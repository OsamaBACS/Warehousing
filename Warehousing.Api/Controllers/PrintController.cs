using Microsoft.AspNetCore.Mvc;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System.Text.Json;

namespace Warehousing.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrintController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<PrintController> _logger;

        public PrintController(IWebHostEnvironment environment, ILogger<PrintController> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        [HttpPost("generate-pdf")]
        public async Task<IActionResult> GeneratePdf([FromBody] PrintRequest request)
        {
            try
            {
                _logger.LogInformation($"Generating PDF for: {request.Title}");
                
                var pdfBytes = await GeneratePdfFromHtml(request.HtmlContent, request.Title, request.Type);
                
                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    return BadRequest("Failed to generate PDF");
                }
                
                return File(pdfBytes, "application/pdf", $"{request.Title}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF");
                return BadRequest($"Error generating PDF: {ex.Message}");
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

        private async Task<byte[]> GeneratePdfFromHtml(string htmlContent, string title, string type)
        {
            try
            {
                _logger.LogInformation("Starting PDF generation for {Title}", title);

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

                    // Set viewport for consistent rendering
                    await page.SetViewportAsync(new ViewPortOptions
                    {
                        Width = 1200,
                        Height = 800
                    });

                    // Navigate to the HTML file
                    var fileUrl = "file://" + tempHtmlPath.Replace("\\", "/");
                    await page.GoToAsync(fileUrl, WaitUntilNavigation.Networkidle0);

                    // Wait a bit for any dynamic content to load
                    await Task.Delay(1000);

                    // Generate PDF
                    var pdfOptions = new PdfOptions
                    {
                        Format = PaperFormat.A4,
                        PrintBackground = true,
                        MarginOptions = new MarginOptions
                        {
                            Top = "20mm",
                            Right = "20mm",
                            Bottom = "20mm",
                            Left = "20mm"
                        },
                        DisplayHeaderFooter = true,
                        HeaderTemplate = GetHeaderTemplate(title),
                        FooterTemplate = GetFooterTemplate()
                    };

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

        private string GetHeaderTemplate(string title)
        {
            return @"
                <div style='font-size: 10px; text-align: center; width: 100%; color: #666;'>
                    <span>" + title + @"</span>
                </div>";
        }

        private string GetFooterTemplate()
        {
            return @"
                <div style='font-size: 10px; text-align: center; width: 100%; color: #666;'>
                    <span>صفحة <span class='pageNumber'></span> من <span class='totalPages'></span></span>
                    <span style='float: left;'>تاريخ الطباعة: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm") + @"</span>
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
