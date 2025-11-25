using Warehousing.Repo.Shared;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Warehousing.Data.Entities;
using System.Security.Claims;

namespace Warehousing.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatusController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public StatusController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetStatusList")]
        public async Task<IActionResult> GetStatusList()
        {
            try
            {
                var list = await _unitOfWork.StatusRepo.GetAll().ToListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("SeedMissingStatuses")]
        [Authorize]
        public async Task<IActionResult> SeedMissingStatuses()
        {
            try
            {
                // Check if user is admin
                var username = User?.FindFirst(ClaimTypes.Name)?.Value;
                var isAdmin = username == "admin" || 
                             User?.HasClaim("IsAdmin", "true") == true ||
                             User?.FindFirst("IsAdmin")?.Value == "true";
                
                if (!isAdmin)
                {
                    return StatusCode(403, new
                    {
                        success = false,
                        message = "Only administrators can seed statuses."
                    });
                }
                // Get existing statuses
                var existingStatuses = await _unitOfWork.StatusRepo.GetAll().ToListAsync();
                var existingCodes = existingStatuses.Select(s => s.Code).ToHashSet();

                // Define all required statuses
                var statuses = new List<Status>
                {
                    new Status { Code = "PENDING", NameEn = "Pending", NameAr = "قيد الانتظار", Description = "Order is created but not processed yet" },
                    new Status { Code = "PROCESSING", NameEn = "Processing", NameAr = "جاري المعالجة", Description = "Order is being prepared or reviewed" },
                    new Status { Code = "CONFIRMED", NameEn = "Confirmed", NameAr = "مؤكد", Description = "Order has been confirmed by the supplier/customer" },
                    new Status { Code = "SHIPPED", NameEn = "Shipped", NameAr = "تم الشحن", Description = "Goods have been dispatched" },
                    new Status { Code = "DELIVERED", NameEn = "Delivered", NameAr = "تم التسليم", Description = "Goods have been successfully delivered" },
                    new Status { Code = "CANCELLED", NameEn = "Cancelled", NameAr = "تم الإلغاء", Description = "Order was cancelled" },
                    new Status { Code = "RETURNED", NameEn = "Returned", NameAr = "تم الإرجاع", Description = "Goods were returned after delivery" },
                    new Status { Code = "COMPLETED", NameEn = "Completed", NameAr = "مكتمل", Description = "Order completed successfully" },
                    new Status { Code = "ONHOLD", NameEn = "On Hold", NameAr = "معلق", Description = "Order temporarily paused" },
                    new Status { Code = "FAILED", NameEn = "Failed", NameAr = "فشل", Description = "Order failed due to payment or stock issue" },
                    new Status { Code = "DRAFT", NameEn = "Save as draft", NameAr = "حفظ كمسودة", Description = "Order is saved but not submitted" }
                };

                // Only add statuses that don't already exist
                var statusesToAdd = statuses.Where(s => !existingCodes.Contains(s.Code)).ToList();
                
                if (statusesToAdd.Any())
                {
                    await _unitOfWork.StatusRepo.CreateRangeAsync(statusesToAdd);
                    
                    return Ok(new
                    {
                        success = true,
                        message = $"Successfully seeded {statusesToAdd.Count} missing statuses.",
                        addedStatuses = statusesToAdd.Select(s => new { s.Code, s.NameEn, s.NameAr })
                    });
                }
                else
                {
                    return Ok(new
                    {
                        success = true,
                        message = "All required statuses already exist in the database.",
                        existingCount = existingStatuses.Count
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error seeding statuses: {ex.Message}"
                });
            }
        }
    }
}