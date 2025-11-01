using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehousing.Data.Entities;
using Warehousing.Repo.Dtos;
using Warehousing.Repo.Interfaces;
using Warehousing.Repo.Services;
using Warehousing.Repo.Shared;

namespace Warehousing.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WorkingHoursController : ControllerBase
    {
        private readonly IWorkingHoursRepo _workingHoursRepo;
        private readonly IActivityLoggingService _activityLoggingService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<WorkingHoursController> _logger;

        public WorkingHoursController(
            IWorkingHoursRepo workingHoursRepo,
            IActivityLoggingService activityLoggingService,
            IUnitOfWork unitOfWork,
            ILogger<WorkingHoursController> logger)
        {
            _workingHoursRepo = workingHoursRepo;
            _activityLoggingService = activityLoggingService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet("GetWorkingHours")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetWorkingHours()
        {
            try
            {
                var workingHours = await _workingHoursRepo.GetActiveWorkingHoursAsync();
                if (workingHours == null)
                {
                    // Create default working hours if none exist
                    workingHours = await _workingHoursRepo.CreateDefaultWorkingHoursAsync();
                }

                if (workingHours == null)
                {
                    return NotFound("No working hours configuration found");
                }

                var dto = new WorkingHoursDto
                {
                    Id = workingHours.Id,
                    Name = workingHours.Name,
                    StartTime = workingHours.StartTime,
                    EndTime = workingHours.EndTime,
                    StartDay = workingHours.StartDay,
                    EndDay = workingHours.EndDay,
                    IsActive = workingHours.IsActive,
                    AllowWeekends = workingHours.AllowWeekends,
                    AllowHolidays = workingHours.AllowHolidays,
                    Description = workingHours.Description,
                    Exceptions = workingHours.Exceptions.Select(e => new WorkingHoursExceptionDto
                    {
                        Id = e.Id,
                        WorkingHoursId = e.WorkingHoursId,
                        ExceptionDate = e.ExceptionDate,
                        StartTime = e.StartTime,
                        EndTime = e.EndTime,
                        Reason = e.Reason,
                        IsWorkingDay = e.IsWorkingDay,
                        CreatedAt = e.CreatedAt ?? DateTime.UtcNow,
                        CreatedBy = e.CreatedBy ?? "system"
                    }).ToList(),
                    CreatedAt = workingHours.CreatedAt ?? DateTime.UtcNow,
                    CreatedBy = workingHours.CreatedBy ?? "system"
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving working hours");
                return StatusCode(500, "An error occurred while retrieving working hours");
            }
        }

        [HttpPost("UpdateWorkingHours")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateWorkingHours([FromBody] WorkingHoursDto dto)
        {
            try
            {
                var existingWorkingHours = await _workingHoursRepo.GetActiveWorkingHoursAsync();
                if (existingWorkingHours == null)
                {
                    return NotFound("No working hours configuration found");
                }

                // Update working hours
                existingWorkingHours.Name = dto.Name;
                existingWorkingHours.StartTime = dto.StartTime;
                existingWorkingHours.EndTime = dto.EndTime;
                existingWorkingHours.StartDay = dto.StartDay;
                existingWorkingHours.EndDay = dto.EndDay;
                existingWorkingHours.AllowWeekends = dto.AllowWeekends;
                existingWorkingHours.AllowHolidays = dto.AllowHolidays;
                existingWorkingHours.Description = dto.Description;
                existingWorkingHours.UpdatedAt = DateTime.UtcNow;
                existingWorkingHours.UpdatedBy = User.FindFirst("UserId")?.Value ?? "system";

                await _unitOfWork.WorkingHoursRepo.UpdateAsync(existingWorkingHours);
                await _unitOfWork.SaveAsync();

                await _activityLoggingService.LogActivityAsync(
                    "UPDATE_WORKING_HOURS",
                    "Working hours configuration updated",
                    "WorkingHours",
                    existingWorkingHours.Id,
                    "",
                    System.Text.Json.JsonSerializer.Serialize(dto),
                    "System"
                );

                return Ok(new { Message = "Working hours updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating working hours");
                return StatusCode(500, "An error occurred while updating working hours");
            }
        }

        [HttpGet("GetWorkingHoursStatus")]
        public async Task<IActionResult> GetWorkingHoursStatus()
        {
            try
            {
                var now = DateTime.Now;
                var isWithinWorkingHours = await _workingHoursRepo.IsWithinWorkingHoursAsync(now);
                var workingHours = await _workingHoursRepo.GetActiveWorkingHoursAsync();

                var status = new WorkingHoursStatusDto
                {
                    IsWithinWorkingHours = isWithinWorkingHours,
                    CurrentTime = now.ToString("HH:mm:ss"),
                    WorkingHoursDescription = workingHours != null 
                        ? $"{workingHours.StartDay} - {workingHours.EndDay}, {workingHours.StartTime:hh\\:mm} - {workingHours.EndTime:hh\\:mm}"
                        : "No working hours configured"
                };

                // Calculate time until next working day if outside working hours
                if (!isWithinWorkingHours && workingHours != null)
                {
                    var nextWorkingDay = CalculateNextWorkingDay(now, workingHours);
                    status.TimeUntilNextWorkingDay = nextWorkingDay - now;
                    status.NextWorkingDay = nextWorkingDay.ToString("dddd, MMMM dd, yyyy");
                }

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving working hours status");
                return StatusCode(500, "An error occurred while retrieving working hours status");
            }
        }

        [HttpPost("AddException")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddException([FromBody] WorkingHoursExceptionDto dto)
        {
            try
            {
                var workingHours = await _workingHoursRepo.GetActiveWorkingHoursAsync();
                if (workingHours == null)
                {
                    return NotFound("No working hours configuration found");
                }

                var exception = new WorkingHoursException
                {
                    WorkingHoursId = workingHours.Id,
                    ExceptionDate = dto.ExceptionDate,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    Reason = dto.Reason,
                    IsWorkingDay = dto.IsWorkingDay,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = User.FindFirst("UserId")?.Value ?? "system"
                };

                _unitOfWork.Context.Set<WorkingHoursException>().Add(exception);
                await _unitOfWork.SaveAsync();

                await _activityLoggingService.LogActivityAsync(
                    "ADD_WORKING_HOURS_EXCEPTION",
                    $"Added exception for {dto.ExceptionDate:yyyy-MM-dd}: {dto.Reason}",
                    "WorkingHoursException",
                    exception.Id,
                    "",
                    System.Text.Json.JsonSerializer.Serialize(dto),
                    "System"
                );

                return Ok(new { Message = "Exception added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding working hours exception");
                return StatusCode(500, "An error occurred while adding exception");
            }
        }

        private DateTime CalculateNextWorkingDay(DateTime currentTime, WorkingHours workingHours)
        {
            var nextDay = currentTime.Date.AddDays(1);
            
            while (true)
            {
                var dayOfWeek = nextDay.DayOfWeek;
                
                // Check if it's a weekend
                if (!workingHours.AllowWeekends && (dayOfWeek == DayOfWeek.Friday || dayOfWeek == DayOfWeek.Saturday))
                {
                    nextDay = nextDay.AddDays(1);
                    continue;
                }

                // Check if it's within working days range
                if (workingHours.StartDay <= workingHours.EndDay)
                {
                    if (dayOfWeek >= workingHours.StartDay && dayOfWeek <= workingHours.EndDay)
                    {
                        return nextDay.Add(workingHours.StartTime);
                    }
                }
                else // Weekend spans across Sunday
                {
                    if (dayOfWeek >= workingHours.StartDay || dayOfWeek <= workingHours.EndDay)
                    {
                        return nextDay.Add(workingHours.StartTime);
                    }
                }

                nextDay = nextDay.AddDays(1);
            }
        }

        [HttpGet("TestWorkingHours")]
        public async Task<IActionResult> TestWorkingHours()
        {
            try
            {
                var currentTime = DateTime.Now;
                var isWithinWorkingHours = await _workingHoursRepo.IsWithinWorkingHoursAsync(currentTime);
                var workingHours = await _workingHoursRepo.GetActiveWorkingHoursAsync();
                
                return Ok(new
                {
                    CurrentTime = currentTime,
                    CurrentTimeOfDay = currentTime.TimeOfDay,
                    CurrentDayOfWeek = currentTime.DayOfWeek,
                    IsWithinWorkingHours = isWithinWorkingHours,
                    WorkingHours = workingHours,
                    User = new
                    {
                        Username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value,
                        IsAdmin = User.IsInRole("Admin") || User.HasClaim("IsAdmin", "true"),
                        Roles = User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList()
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing working hours");
                return StatusCode(500, "An error occurred while testing working hours.");
            }
        }
    }
}
