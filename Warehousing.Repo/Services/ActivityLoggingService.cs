using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Warehousing.Repo.Interfaces;

namespace Warehousing.Repo.Services
{
    public interface IActivityLoggingService
    {
        Task LogActivityAsync(string action, string description, string entityType = "", int? entityId = null, string oldValues = "", string newValues = "", string module = "", string severity = "INFO");
        Task LogLoginAsync(int userId, string ipAddress);
        Task LogLogoutAsync(int userId, string ipAddress);
        Task LogCreateAsync(string entityType, int entityId, string description, string module = "");
        Task LogUpdateAsync(string entityType, int entityId, string description, string oldValues, string newValues, string module = "");
        Task LogDeleteAsync(string entityType, int entityId, string description, string module = "");
    }

    public class ActivityLoggingService : IActivityLoggingService
    {
        private readonly IUserActivityLogRepo _activityLogRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ActivityLoggingService(IUserActivityLogRepo activityLogRepo, IHttpContextAccessor httpContextAccessor)
        {
            _activityLogRepo = activityLogRepo;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogActivityAsync(string action, string description, string entityType = "", int? entityId = null, string oldValues = "", string newValues = "", string module = "", string severity = "INFO")
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return;

                var ipAddress = GetClientIpAddress();
                var userAgent = GetUserAgent();

                await _activityLogRepo.LogUserActivityAsync(
                    userId.Value,
                    action,
                    description,
                    entityType,
                    entityId,
                    oldValues,
                    newValues,
                    ipAddress,
                    userAgent,
                    module,
                    severity
                );
            }
            catch (Exception ex)
            {
                // Log error but don't throw to avoid breaking the main operation
                Console.WriteLine($"Error logging activity: {ex.Message}");
            }
        }

        public async Task LogLoginAsync(int userId, string ipAddress)
        {
            var userAgent = GetUserAgent();
            await _activityLogRepo.LogUserActivityAsync(
                userId,
                "LOGIN",
                "User logged in successfully",
                "User",
                userId,
                "",
                "",
                ipAddress,
                userAgent,
                "Authentication",
                "INFO"
            );
        }

        public async Task LogLogoutAsync(int userId, string ipAddress)
        {
            var userAgent = GetUserAgent();
            await _activityLogRepo.LogUserActivityAsync(
                userId,
                "LOGOUT",
                "User logged out",
                "User",
                userId,
                "",
                "",
                ipAddress,
                userAgent,
                "Authentication",
                "INFO"
            );
        }

        public async Task LogCreateAsync(string entityType, int entityId, string description, string module = "")
        {
            await LogActivityAsync("CREATE", description, entityType, entityId, "", "", module, "INFO");
        }

        public async Task LogUpdateAsync(string entityType, int entityId, string description, string oldValues, string newValues, string module = "")
        {
            await LogActivityAsync("UPDATE", description, entityType, entityId, oldValues, newValues, module, "INFO");
        }

        public async Task LogDeleteAsync(string entityType, int entityId, string description, string module = "")
        {
            await LogActivityAsync("DELETE", description, entityType, entityId, "", "", module, "WARNING");
        }

        private int? GetCurrentUserId()
        {
            try
            {
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                    return userId;
                return null;
            }
            catch
            {
                return null;
            }
        }

        private string GetClientIpAddress()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null) return "";

                // Check for forwarded IP first
                var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    var ip = forwardedFor.Split(',')[0].Trim();
                    if (IsValidIpAddress(ip))
                        return ip;
                }

                // Check for real IP
                var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
                if (!string.IsNullOrEmpty(realIp) && IsValidIpAddress(realIp))
                    return realIp;

                // Fall back to remote IP
                var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString();
                return remoteIp ?? "";
            }
            catch
            {
                return "";
            }
        }

        private string GetUserAgent()
        {
            try
            {
                return _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].FirstOrDefault() ?? "";
            }
            catch
            {
                return "";
            }
        }

        private bool IsValidIpAddress(string ip)
        {
            return System.Net.IPAddress.TryParse(ip, out _);
        }
    }
}
