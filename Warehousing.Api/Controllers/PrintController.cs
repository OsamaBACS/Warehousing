using Microsoft.AspNetCore.Mvc;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System.Text.Json;
using Warehousing.Repo.Shared;
using Warehousing.Data.Models;
using PrinterConfigEntity = Warehousing.Data.Entities.PrinterConfiguration;
using Microsoft.EntityFrameworkCore;
using Warehousing.Api.Services;
using System.Security.Claims;

namespace Warehousing.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrintController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<PrintController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly EscPosService _escPosService;

        public PrintController(IWebHostEnvironment environment, ILogger<PrintController> logger, IUnitOfWork unitOfWork, EscPosService escPosService)
        {
            _environment = environment;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _escPosService = escPosService;
        }

        [HttpPost("generate-pdf")]
        public async Task<IActionResult> GeneratePdf([FromBody] PrintRequest request)
        {
            try
            {
                _logger.LogInformation($"Generating print output for: {request.Title}");
                
                var printerConfig = await GetPrinterConfiguration();
                
                // Check if this is a POS/Thermal printer with ESC/POS enabled
                if ((printerConfig.PrinterType == "POS" || printerConfig.PrinterType == "Thermal") 
                    && printerConfig.PosSettings?.UseEscPos == true)
                {
                    var escPosBytes = _escPosService.GenerateEscPosCommands(request.HtmlContent, printerConfig);
                    if (escPosBytes == null || escPosBytes.Length == 0)
                    {
                        return BadRequest("Failed to generate ESC/POS commands");
                    }
                    return File(escPosBytes, "application/octet-stream", $"{request.Title}.escpos");
                }
                
                // Default to PDF generation
                var pdfBytes = await GeneratePdfFromHtml(request.HtmlContent, request.Title, request.Type);
                
                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    return BadRequest("Failed to generate PDF");
                }
                
                return File(pdfBytes, "application/pdf", $"{request.Title}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating print output");
                return BadRequest($"Error generating print output: {ex.Message}");
            }
        }

        [HttpPost("generate-order-pdf")]
        public async Task<IActionResult> GenerateOrderPdf([FromBody] OrderPrintRequest request)
        {
            try
            {
                _logger.LogInformation($"Generating order PDF for order ID: {request.OrderId}");
                
                // Generate HTML content for order
                var htmlContent = GenerateOrderHtml(request);
                var pdfBytes = await GeneratePdfFromHtml(htmlContent, $"Order-{request.OrderId}", "order");
                
                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    return BadRequest("Failed to generate PDF");
                }
                
                return File(pdfBytes, "application/pdf", $"Order-{request.OrderId}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating order PDF");
                return BadRequest($"Error generating order PDF: {ex.Message}");
            }
        }

        [HttpPost("generate-report-pdf")]
        public async Task<IActionResult> GenerateReportPdf([FromBody] ReportPrintRequest request)
        {
            try
            {
                _logger.LogInformation($"Generating report PDF: {request.ReportType}");
                
                var htmlContent = GenerateReportHtml(request);
                var pdfBytes = await GeneratePdfFromHtml(htmlContent, request.ReportType, "report");
                
                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    return BadRequest("Failed to generate PDF");
                }
                
                return File(pdfBytes, "application/pdf", $"{request.ReportType}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report PDF");
                return BadRequest($"Error generating report PDF: {ex.Message}");
            }
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            try
            {
                return Ok(new { 
                    status = "healthy", 
                    message = "PDF service is available",
                    timestamp = DateTime.UtcNow,
                    version = "1.0.0"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(500, new { 
                    status = "unhealthy", 
                    message = "PDF service is not available",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Test endpoint to save ESC/POS commands to a file for inspection
        /// This is useful for testing POS printer configurations without a physical printer
        /// </summary>
        [HttpPost("test-escpos")]
        public async Task<IActionResult> TestEscPos([FromBody] PrintRequest request)
        {
            try
            {
                _logger.LogInformation($"Testing ESC/POS generation for: {request.Title}");
                
                var printerConfig = await GetPrinterConfiguration();
                
                // Generate ESC/POS commands
                var escPosBytes = _escPosService.GenerateEscPosCommands(request.HtmlContent, printerConfig);
                
                if (escPosBytes == null || escPosBytes.Length == 0)
                {
                    return BadRequest("Failed to generate ESC/POS commands");
                }

                // Save to a test file in the wwwroot/test-prints directory
                var testDir = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "test-prints");
                if (!Directory.Exists(testDir))
                {
                    Directory.CreateDirectory(testDir);
                }

                var fileName = $"{request.Title}_{DateTime.Now:yyyyMMdd_HHmmss}.escpos";
                var filePath = Path.Combine(testDir, fileName);
                await System.IO.File.WriteAllBytesAsync(filePath, escPosBytes);

                // Also create a hex dump for inspection
                var hexDump = BitConverter.ToString(escPosBytes).Replace("-", " ");
                var hexFilePath = Path.Combine(testDir, $"{request.Title}_{DateTime.Now:yyyyMMdd_HHmmss}.hex");
                await System.IO.File.WriteAllTextAsync(hexFilePath, hexDump);

                return Ok(new
                {
                    message = "ESC/POS commands generated and saved",
                    filePath = filePath,
                    hexFilePath = hexFilePath,
                    fileSize = escPosBytes.Length,
                    downloadUrl = $"/test-prints/{fileName}",
                    hexDownloadUrl = $"/test-prints/{Path.GetFileName(hexFilePath)}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing ESC/POS generation");
                return BadRequest($"Error testing ESC/POS: {ex.Message}");
            }
        }

        private async Task<byte[]> GeneratePdfFromHtml(string htmlContent, string title, string type)
        {
            try
            {
                _logger.LogInformation("Starting PDF generation for {Title}", title);

                // Get company printer configuration
                var printerConfig = await GetPrinterConfiguration();

                // For POS/Thermal printers, generate ESC/POS commands instead of PDF
                if (printerConfig.PrinterType == "POS" || printerConfig.PrinterType == "Thermal")
                {
                    if (printerConfig.PosSettings?.UseEscPos == true)
                    {
                        _logger.LogInformation("Generating ESC/POS commands for {PrinterType} printer", printerConfig.PrinterType);
                        var escPosBytes = _escPosService.GenerateEscPosCommands(htmlContent, printerConfig);
                        return escPosBytes;
                    }
                    else
                    {
                        _logger.LogWarning("ESC/POS is disabled for POS printer. Falling back to PDF.");
                    }
                }

                // Create a temporary HTML file
                var tempHtmlPath = Path.GetTempFileName() + ".html";
                await System.IO.File.WriteAllTextAsync(tempHtmlPath, htmlContent, System.Text.Encoding.UTF8);

                try
                {
                // Launch Puppeteer
                var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();

                    var launchOptions = new LaunchOptions
                    {
                        Headless = true,
                        Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
                    };

                    using var browser = await PuppeteerSharp.Puppeteer.LaunchAsync(launchOptions);
                    using var page = await browser.NewPageAsync();

                    // Set viewport based on printer configuration
                    var viewportWidth = printerConfig.PrinterType == "POS" || printerConfig.PrinterType == "Thermal" 
                        ? printerConfig.PaperWidth * 4 // Convert mm to pixels (approximate)
                        : 1200;
                    var viewportHeight = printerConfig.PrinterType == "POS" || printerConfig.PrinterType == "Thermal"
                        ? 800
                        : 800;

                    await page.SetViewportAsync(new ViewPortOptions
                    {
                        Width = viewportWidth,
                        Height = viewportHeight
                    });

                    // Navigate to the HTML file
                    var fileUrl = "file://" + tempHtmlPath.Replace("\\", "/");
                    await page.GoToAsync(fileUrl, WaitUntilNavigation.Networkidle0);

                    // Wait a bit for any dynamic content to load
                    await Task.Delay(1000);

                    // Generate PDF with configured settings
                    var pdfOptions = CreatePdfOptions(printerConfig, title);
                    var pdfBytes = await page.PdfDataAsync(pdfOptions);
                    _logger.LogInformation("PDF generated successfully, size: {Size} bytes", pdfBytes.Length);

                    return pdfBytes;
                }
                finally
                {
                    // Clean up temporary file
                    if (System.IO.File.Exists(tempHtmlPath))
                    {
                        System.IO.File.Delete(tempHtmlPath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF from HTML");
                return new byte[0];
            }
        }

        private async Task<PrinterConfiguration> GetPrinterConfiguration()
        {
            try
            {
                // Get current user ID from claims
                var userIdClaim = User?.FindFirst("UserId")?.Value 
                    ?? User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning("Could not determine user ID from claims, using default A4 configuration");
                    return GetDefaultA4Configuration();
                }

                // Get user with roles and their printer configurations
                var user = await _unitOfWork.UserRepo.GetAll()
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                            .ThenInclude(r => r != null ? r.PrinterConfiguration : null!)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found, using default A4 configuration", userId);
                    return GetDefaultA4Configuration();
                }

                // Find the first role with a printer configuration
                var roleWithConfig = user.UserRoles
                    .Select(ur => ur.Role)
                    .FirstOrDefault(r => r?.PrinterConfigurationId != null && r.PrinterConfiguration != null);

                if (roleWithConfig?.PrinterConfiguration == null)
                {
                    _logger.LogInformation("No printer configuration found for user {UserId}, using default A4 configuration", userId);
                    return GetDefaultA4Configuration();
                }

                var dbConfig = roleWithConfig.PrinterConfiguration!;
                
                // Convert database entity to model
                var config = new PrinterConfiguration
                {
                    PrinterType = dbConfig.PrinterType,
                    PaperFormat = dbConfig.PaperFormat,
                    PaperWidth = dbConfig.PaperWidth,
                    PaperHeight = dbConfig.PaperHeight,
                    PrintInColor = dbConfig.PrintInColor,
                    PrintBackground = dbConfig.PrintBackground,
                    Orientation = dbConfig.Orientation,
                    Scale = dbConfig.Scale
                };

                // Deserialize JSON fields
                if (!string.IsNullOrEmpty(dbConfig.Margins))
                {
                    try
                    {
                        config.Margins = JsonSerializer.Deserialize<PrinterMargins>(dbConfig.Margins) 
                            ?? new PrinterMargins();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to deserialize margins, using defaults");
                        config.Margins = new PrinterMargins();
                    }
                }
                else
                {
                    config.Margins = new PrinterMargins();
                }

                if (!string.IsNullOrEmpty(dbConfig.FontSettings))
                {
                    try
                    {
                        config.FontSettings = JsonSerializer.Deserialize<PrinterFontSettings>(dbConfig.FontSettings) 
                            ?? new PrinterFontSettings();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to deserialize font settings, using defaults");
                        config.FontSettings = new PrinterFontSettings();
                    }
                }
                else
                {
                    config.FontSettings = new PrinterFontSettings();
                }

                if (!string.IsNullOrEmpty(dbConfig.PosSettings))
                {
                    try
                    {
                        config.PosSettings = JsonSerializer.Deserialize<PosPrinterSettings>(dbConfig.PosSettings);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to deserialize POS settings");
                    }
                }

                _logger.LogInformation("Loaded printer configuration '{ConfigName}' for user {UserId}", 
                    dbConfig.NameAr, userId);
                
                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading printer configuration, using defaults");
                return GetDefaultA4Configuration();
            }
        }

        private PrinterConfiguration GetDefaultA4Configuration()
        {
            return new PrinterConfiguration
            {
                PrinterType = "A4",
                PaperFormat = "A4",
                PaperWidth = 210,
                PaperHeight = 297,
                Margins = new PrinterMargins
                {
                    Top = "20mm",
                    Right = "20mm",
                    Bottom = "20mm",
                    Left = "20mm"
                },
                FontSettings = new PrinterFontSettings
                {
                    FontFamily = "Arial",
                    BaseFontSize = 12,
                    HeaderFontSize = 16,
                    FooterFontSize = 10,
                    TableFontSize = 11
                },
                PrintInColor = true,
                PrintBackground = true,
                Orientation = "Portrait",
                Scale = 1.0
            };
        }

        private PdfOptions CreatePdfOptions(PrinterConfiguration config, string title)
        {
            // Map paper format
            PaperFormat paperFormat = config.PaperFormat switch
            {
                "A4" => PaperFormat.A4,
                "Letter" => PaperFormat.Letter,
                "Legal" => PaperFormat.Legal,
                "A3" => PaperFormat.A3,
                "A5" => PaperFormat.A5,
                _ => PaperFormat.A4
            };

            // For POS/Thermal printers, use custom dimensions
            if (config.PrinterType == "POS" || config.PrinterType == "Thermal")
            {
                // Convert mm to inches for Puppeteer
                var widthInches = config.PaperWidth / 25.4;
                var heightInches = config.PaperHeight > 0 ? config.PaperHeight / 25.4 : 11.0; // Default to Letter height if continuous

                return new PdfOptions
                {
                    Width = $"{widthInches}in",
                    Height = config.PaperHeight > 0 ? $"{heightInches}in" : null,
                    PrintBackground = config.PrintBackground,
                    MarginOptions = new MarginOptions
                    {
                        Top = config.Margins.Top,
                        Right = config.Margins.Right,
                        Bottom = config.Margins.Bottom,
                        Left = config.Margins.Left
                    },
                    DisplayHeaderFooter = false, // Usually not used for POS receipts
                    Scale = (decimal)config.Scale,
                    PreferCSSPageSize = true
                };
            }

            // Standard A4/Label printer options
            return new PdfOptions
            {
                Format = paperFormat,
                PrintBackground = config.PrintBackground,
                MarginOptions = new MarginOptions
                {
                    Top = config.Margins.Top,
                    Right = config.Margins.Right,
                    Bottom = config.Margins.Bottom,
                    Left = config.Margins.Left
                },
                DisplayHeaderFooter = true,
                HeaderTemplate = GetHeaderTemplate(title, config),
                FooterTemplate = GetFooterTemplate(config),
                Landscape = config.Orientation == "Landscape",
                Scale = (decimal)config.Scale
            };
        }

        private string GetHeaderTemplate(string title, PrinterConfiguration? config = null)
        {
            var fontSize = config?.FontSettings?.HeaderFontSize ?? 10;
            return $@"
                <div style='font-size: {fontSize}px; text-align: center; width: 100%; color: #666;'>
                    <span>{title}</span>
                </div>";
        }

        private string GetFooterTemplate(PrinterConfiguration? config = null)
        {
            var fontSize = config?.FontSettings?.FooterFontSize ?? 10;
            return $@"
                <div style='font-size: {fontSize}px; text-align: center; width: 100%; color: #666;'>
                    <span>صفحة <span class='pageNumber'></span> من <span class='totalPages'></span></span>
                    <span style='float: left;'>تاريخ الطباعة: {DateTime.Now:yyyy-MM-dd HH:mm}</span>
                </div>";
        }

        private string GenerateOrderHtml(OrderPrintRequest request)
        {
            // This would generate HTML based on order data
            // You can create templates or use a templating engine like Razor
            return $@"
                <!DOCTYPE html>
                <html dir='rtl'>
                <head>
                    <meta charset='UTF-8'>
                    <title>Order {request.OrderId}</title>
                    <style>
                        body {{ font-family: 'Segoe UI', Arial, sans-serif; margin: 0; padding: 20px; }}
                        .header {{ text-align: center; margin-bottom: 30px; }}
                        .order-info {{ display: flex; justify-content: space-between; margin-bottom: 20px; }}
                        .order-info div {{ background: #f8f9fa; padding: 15px; border-radius: 8px; }}
                        table {{ width: 100%; border-collapse: collapse; margin: 20px 0; }}
                        th, td {{ border: 1px solid #ddd; padding: 12px; text-align: right; }}
                        th {{ background: #f2f2f2; }}
                        .total {{ background: #e8f5e8; padding: 20px; text-align: center; font-size: 18px; font-weight: bold; }}
                    </style>
                </head>
                <body>
                    <div class='header'>
                        <h1>طلب رقم {request.OrderId}</h1>
                        <p>تاريخ الطلب: {request.OrderDate:yyyy-MM-dd}</p>
                    </div>
                    
                    <div class='order-info'>
                        <div>
                            <strong>العميل:</strong> {request.CustomerName}
                        </div>
                        <div>
                            <strong>المجموع:</strong> {request.TotalAmount:C} د.أ
                        </div>
                    </div>
                    
                    <table>
                        <thead>
                            <tr>
                                <th>المنتج</th>
                                <th>الكمية</th>
                                <th>السعر</th>
                                <th>المجموع</th>
                            </tr>
                        </thead>
                        <tbody>
                            {string.Join("", request.Items.Select(item => $@"
                                <tr>
                                    <td>{item.ProductName}</td>
                                    <td>{item.Quantity}</td>
                                    <td>{item.UnitPrice:C} د.أ</td>
                                    <td>{item.Total:C} د.أ</td>
                                </tr>"))}
                        </tbody>
                    </table>
                    
                    <div class='total'>
                        المجموع الكلي: {request.TotalAmount:C} د.أ
                    </div>
                </body>
                </html>";
        }

        private string GenerateReportHtml(ReportPrintRequest request)
        {
            // Generate HTML for reports
            return $@"
                <!DOCTYPE html>
                <html dir='rtl'>
                <head>
                    <meta charset='UTF-8'>
                    <title>{request.ReportType}</title>
                    <style>
                        body {{ font-family: 'Segoe UI', Arial, sans-serif; margin: 0; padding: 20px; }}
                        .header {{ text-align: center; margin-bottom: 30px; }}
                        .report-info {{ margin-bottom: 20px; }}
                        table {{ width: 100%; border-collapse: collapse; margin: 20px 0; }}
                        th, td {{ border: 1px solid #ddd; padding: 12px; text-align: right; }}
                        th {{ background: #f2f2f2; }}
                    </style>
                </head>
                <body>
                    <div class='header'>
                        <h1>{request.ReportType}</h1>
                        <p>تاريخ التقرير: {DateTime.Now:yyyy-MM-dd}</p>
                    </div>
                    
                    <div class='report-info'>
                        <p><strong>نوع التقرير:</strong> {request.ReportType}</p>
                        <p><strong>الفترة:</strong> {request.StartDate:yyyy-MM-dd} إلى {request.EndDate:yyyy-MM-dd}</p>
                    </div>
                    
                    <!-- Report content would be generated here -->
                    <p>محتوى التقرير...</p>
                </body>
                </html>";
        }
    }

    public class PrintRequest
    {
        public string HtmlContent { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = "document";
    }

    public class OrderPrintRequest
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
    }

    public class ReportPrintRequest
    {
        public string ReportType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new();
    }
}
