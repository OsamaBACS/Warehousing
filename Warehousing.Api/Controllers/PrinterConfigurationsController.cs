using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Warehousing.Data.Entities;
using Warehousing.Repo.Dtos;
using Warehousing.Repo.Shared;
using System.Text.Json;

namespace Warehousing.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrinterConfigurationsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PrinterConfigurationsController> _logger;

        public PrinterConfigurationsController(IUnitOfWork unitOfWork, ILogger<PrinterConfigurationsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet]
        [Route("GetPrinterConfigurations")]
        public async Task<IActionResult> GetPrinterConfigurations()
        {
            try
            {
                var configs = await _unitOfWork.Context.PrinterConfigurations
                    .Where(pc => pc.IsActive)
                    .Select(pc => new PrinterConfigurationDto
                    {
                        Id = pc.Id,
                        NameAr = pc.NameAr,
                        NameEn = pc.NameEn,
                        Description = pc.Description,
                        PrinterType = pc.PrinterType,
                        PaperFormat = pc.PaperFormat,
                        PaperWidth = pc.PaperWidth,
                        PaperHeight = pc.PaperHeight,
                        Margins = pc.Margins,
                        FontSettings = pc.FontSettings,
                        PosSettings = pc.PosSettings,
                        PrintInColor = pc.PrintInColor,
                        PrintBackground = pc.PrintBackground,
                        Orientation = pc.Orientation,
                        Scale = pc.Scale,
                        IsActive = pc.IsActive,
                        IsDefault = pc.IsDefault
                    })
                    .ToListAsync();

                return Ok(configs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting printer configurations");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("GetPrinterConfigurationById")]
        public async Task<IActionResult> GetPrinterConfigurationById(int id)
        {
            try
            {
                var config = await _unitOfWork.Context.PrinterConfigurations
                    .FirstOrDefaultAsync(pc => pc.Id == id);

                if (config == null)
                {
                    return NotFound("Printer configuration not found");
                }

                var dto = new PrinterConfigurationDto
                {
                    Id = config.Id,
                    NameAr = config.NameAr,
                    NameEn = config.NameEn,
                    Description = config.Description,
                    PrinterType = config.PrinterType,
                    PaperFormat = config.PaperFormat,
                    PaperWidth = config.PaperWidth,
                    PaperHeight = config.PaperHeight,
                    Margins = config.Margins,
                    FontSettings = config.FontSettings,
                    PosSettings = config.PosSettings,
                    PrintInColor = config.PrintInColor,
                    PrintBackground = config.PrintBackground,
                    Orientation = config.Orientation,
                    Scale = config.Scale,
                    IsActive = config.IsActive,
                    IsDefault = config.IsDefault
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting printer configuration by ID");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("SavePrinterConfiguration")]
        public async Task<IActionResult> SavePrinterConfiguration([FromBody] PrinterConfigurationDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest("Printer configuration model is null");
                }

                PrinterConfiguration config;

                if (dto.Id > 0)
                {
                    // Update existing
                    config = await _unitOfWork.Context.PrinterConfigurations
                        .FirstOrDefaultAsync(pc => pc.Id == dto.Id);

                    if (config == null)
                    {
                        return NotFound("Printer configuration not found");
                    }

                    if (config.IsDefault)
                    {
                        return BadRequest("Cannot modify default printer configuration");
                    }

                    config.NameAr = dto.NameAr;
                    config.NameEn = dto.NameEn;
                    config.Description = dto.Description;
                    config.PrinterType = dto.PrinterType;
                    config.PaperFormat = dto.PaperFormat;
                    config.PaperWidth = dto.PaperWidth;
                    config.PaperHeight = dto.PaperHeight;
                    config.Margins = dto.Margins;
                    config.FontSettings = dto.FontSettings;
                    config.PosSettings = dto.PosSettings;
                    config.PrintInColor = dto.PrintInColor;
                    config.PrintBackground = dto.PrintBackground;
                    config.Orientation = dto.Orientation;
                    config.Scale = dto.Scale;
                    config.IsActive = dto.IsActive;
                    config.UpdatedAt = DateTime.UtcNow;
                    config.UpdatedBy = User?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "system";

                    _unitOfWork.Context.PrinterConfigurations.Update(config);
                }
                else
                {
                    // Create new
                    config = new PrinterConfiguration
                    {
                        NameAr = dto.NameAr,
                        NameEn = dto.NameEn,
                        Description = dto.Description,
                        PrinterType = dto.PrinterType,
                        PaperFormat = dto.PaperFormat,
                        PaperWidth = dto.PaperWidth,
                        PaperHeight = dto.PaperHeight,
                        Margins = dto.Margins,
                        FontSettings = dto.FontSettings,
                        PosSettings = dto.PosSettings,
                        PrintInColor = dto.PrintInColor,
                        PrintBackground = dto.PrintBackground,
                        Orientation = dto.Orientation,
                        Scale = dto.Scale,
                        IsActive = dto.IsActive,
                        IsDefault = false,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = User?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "system"
                    };

                    await _unitOfWork.Context.PrinterConfigurations.AddAsync(config);
                }

                await _unitOfWork.Context.SaveChangesAsync();

                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving printer configuration");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete]
        [Route("DeletePrinterConfiguration")]
        public async Task<IActionResult> DeletePrinterConfiguration(int id)
        {
            try
            {
                var config = await _unitOfWork.Context.PrinterConfigurations
                    .Include(pc => pc.Roles)
                    .FirstOrDefaultAsync(pc => pc.Id == id);

                if (config == null)
                {
                    return NotFound("Printer configuration not found");
                }

                if (config.IsDefault)
                {
                    return BadRequest("Cannot delete default printer configuration");
                }

                if (config.Roles.Any())
                {
                    return BadRequest("Cannot delete printer configuration that is assigned to roles. Please unassign it first.");
                }

                _unitOfWork.Context.PrinterConfigurations.Remove(config);
                await _unitOfWork.Context.SaveChangesAsync();

                return Ok(new { message = "Printer configuration deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting printer configuration");
                return StatusCode(500, ex.Message);
            }
        }
    }
}

