using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehousing.Repo.Dtos;
using Warehousing.Repo.Interfaces;
using Warehousing.Repo.Services;
using Warehousing.Repo.Shared;

namespace Warehousing.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ActivityLogController : ControllerBase
    {
        private readonly IUserActivityLogRepo _activityLogRepo;
        private readonly IActivityLoggingService _activityLoggingService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ActivityLogController> _logger;

        public ActivityLogController(
            IUserActivityLogRepo activityLogRepo,
            IActivityLoggingService activityLoggingService,
            IUnitOfWork unitOfWork,
            ILogger<ActivityLogController> logger)
        {
            _activityLogRepo = activityLogRepo;
            _activityLoggingService = activityLoggingService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet("GetUserActivityLogs")]
        public async Task<IActionResult> GetUserActivityLogs([FromQuery] int userId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                var logs = await _activityLogRepo.GetUserActivityLogsAsync(userId, startDate, endDate, page, pageSize);
                var totalCount = await _activityLogRepo.GetActivityLogCountAsync(userId, startDate ?? DateTime.MinValue, endDate ?? DateTime.MaxValue);

                var result = logs.Select(log => new UserActivityLogDto
                {
                    Id = log.Id,
                    UserId = log.UserId,
                    UserName = log.User?.Username ?? "Unknown",
                    Action = log.Action,
                    Description = log.Description,
                    EntityType = log.EntityType,
                    EntityId = log.EntityId,
                    OldValues = log.OldValues,
                    NewValues = log.NewValues,
                    IpAddress = log.IpAddress,
                    UserAgent = log.UserAgent,
                    Module = log.Module,
                    Severity = log.Severity,
                    Timestamp = log.Timestamp,
                    CreatedAt = log.CreatedAt ?? DateTime.UtcNow,
                    CreatedBy = log.CreatedBy ?? "system"
                }).ToList();

                return Ok(new
                {
                    Data = result,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user activity logs");
                return StatusCode(500, "An error occurred while retrieving activity logs");
            }
        }

        [HttpGet("GetAllActivityLogs")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllActivityLogs([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] string? action = null, [FromQuery] string? module = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                var logs = await _activityLogRepo.GetAllActivityLogsAsync(startDate, endDate, action, module, page, pageSize);
                var totalCount = await _activityLogRepo.GetActivityLogCountAsync(null, startDate ?? DateTime.MinValue, endDate ?? DateTime.MaxValue);

                var result = logs.Select(log => new UserActivityLogDto
                {
                    Id = log.Id,
                    UserId = log.UserId,
                    UserName = log.User?.Username ?? "Unknown",
                    Action = log.Action,
                    Description = log.Description,
                    EntityType = log.EntityType,
                    EntityId = log.EntityId,
                    OldValues = log.OldValues,
                    NewValues = log.NewValues,
                    IpAddress = log.IpAddress,
                    UserAgent = log.UserAgent,
                    Module = log.Module,
                    Severity = log.Severity,
                    Timestamp = log.Timestamp,
                    CreatedAt = log.CreatedAt ?? DateTime.UtcNow,
                    CreatedBy = log.CreatedBy ?? "system"
                }).ToList();

                return Ok(new
                {
                    Data = result,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all activity logs");
                return StatusCode(500, "An error occurred while retrieving activity logs");
            }
        }

        [HttpGet("GetActivitySummary")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetActivitySummary()
        {
            try
            {
                var now = DateTime.UtcNow;
                var today = now.Date;
                var weekStart = today.AddDays(-(int)today.DayOfWeek);
                var monthStart = new DateTime(today.Year, today.Month, 1);

                var totalLogs = await _activityLogRepo.GetActivityLogCountAsync();
                var todayLogs = await _activityLogRepo.GetActivityLogCountAsync(null, today, now);
                var weekLogs = await _activityLogRepo.GetActivityLogCountAsync(null, weekStart, now);
                var monthLogs = await _activityLogRepo.GetActivityLogCountAsync(null, monthStart, now);

                // Get recent logs for analysis
                var recentLogs = await _activityLogRepo.GetAllActivityLogsAsync(null, null, null, null, 1, 1000);

                var actionCounts = recentLogs
                    .GroupBy(log => log.Action)
                    .ToDictionary(g => g.Key, g => g.Count());

                var moduleCounts = recentLogs
                    .GroupBy(log => log.Module)
                    .ToDictionary(g => g.Key, g => g.Count());

                var severityCounts = recentLogs
                    .GroupBy(log => log.Severity)
                    .ToDictionary(g => g.Key, g => g.Count());

                var summary = new ActivityLogSummaryDto
                {
                    TotalLogs = totalLogs,
                    TodayLogs = todayLogs,
                    ThisWeekLogs = weekLogs,
                    ThisMonthLogs = monthLogs,
                    ActionCounts = actionCounts,
                    ModuleCounts = moduleCounts,
                    SeverityCounts = severityCounts
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activity summary");
                return StatusCode(500, "An error occurred while retrieving activity summary");
            }
        }

        [HttpDelete("ClearOldLogs")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ClearOldLogs([FromQuery] int daysToKeep = 90)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
                var oldLogs = await _activityLogRepo.GetAllActivityLogsAsync(null, null, null, null, 1, int.MaxValue);
                var logsToDelete = oldLogs.Where(log => log.Timestamp < cutoffDate).ToList();

                foreach (var log in logsToDelete)
                {
                    await _unitOfWork.UserActivityLogRepo.DeleteAsync(log);
                }

                await _unitOfWork.SaveAsync();

                await _activityLoggingService.LogActivityAsync(
                    "CLEAR_LOGS",
                    $"Cleared {logsToDelete.Count} old activity logs older than {daysToKeep} days",
                    "ActivityLog",
                    null,
                    "",
                    "",
                    "System"
                );

                return Ok(new { Message = $"Successfully cleared {logsToDelete.Count} old logs" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing old logs");
                return StatusCode(500, "An error occurred while clearing old logs");
            }
        }
    }
}
