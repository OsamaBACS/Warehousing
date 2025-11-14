using System.Security.Claims;
using Warehousing.Repo.Interfaces;

namespace Warehousing.Api.Middlewares
{
    public class WorkingHoursMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<WorkingHoursMiddleware> _logger;

        public WorkingHoursMiddleware(RequestDelegate next, ILogger<WorkingHoursMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IWorkingHoursRepo workingHoursRepo)
        {
            try
            {
                // Skip working hours check for certain paths
                var path = context.Request.Path.Value?.ToLower() ?? "";
                if (ShouldSkipWorkingHoursCheck(path))
                {
                    await _next(context);
                    return;
                }

                // Check if user is authenticated
                if (!context.User.Identity?.IsAuthenticated ?? true)
                {
                    await _next(context);
                    return;
                }

                // Check if user is admin (admins can access anytime)
                // Only allow admin users with the specific "IsAdmin" claim or username "admin"
                var username = context.User.FindFirst(ClaimTypes.Name)?.Value;
                var isAdmin = username == "admin" || 
                             context.User.HasClaim("IsAdmin", "true") ||
                             context.User.FindFirst("IsAdmin")?.Value == "true";

                if (isAdmin)
                {
                    await _next(context);
                    return;
                }

                // Check if user has permission to work outside working hours
                // Permissions are stored as comma-separated string in the "Permission" claim
                var permissionClaim = context.User.FindFirst("Permission")?.Value ?? string.Empty;
                
                // Split permissions and check more carefully
                var permissionList = string.IsNullOrEmpty(permissionClaim) 
                    ? new List<string>() 
                    : permissionClaim.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(p => p.Trim())
                        .ToList();
                
                // Log permissions for debugging
                _logger.LogInformation("User {Username} - Total permissions in token: {Count}, Permissions: {Permissions}", 
                    username ?? "Unknown", 
                    permissionList.Count,
                    string.IsNullOrEmpty(permissionClaim) ? "NONE" : permissionClaim);
                
                // Check if WORK_OUTSIDE_WORKING_HOURS is in the permission list (exact match with case-insensitive comparison)
                var hasWorkOutsidePermission = permissionList.Any(p => 
                    string.Equals(p, "WORK_OUTSIDE_WORKING_HOURS", StringComparison.OrdinalIgnoreCase));
                
                _logger.LogInformation("User {Username} - WORK_OUTSIDE_WORKING_HOURS check: {HasPermission} (checked {Count} permissions)", 
                    username, hasWorkOutsidePermission, permissionList.Count);

                if (hasWorkOutsidePermission)
                {
                    _logger.LogInformation("User {Username} has WORK_OUTSIDE_WORKING_HOURS permission, allowing access", username);
                    await _next(context);
                    return;
                }
                
                _logger.LogInformation("User {Username} does NOT have WORK_OUTSIDE_WORKING_HOURS permission. Permission claim value: '{PermissionClaim}'", 
                    username, permissionClaim);

                // Check working hours for users without the permission
                var isWithinWorkingHours = await workingHoursRepo.IsWithinWorkingHoursAsync(DateTime.Now);
                
                if (!isWithinWorkingHours)
                {
                    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? context.User.FindFirst("UserId")?.Value;
                    _logger.LogWarning("Access denied for user {Username} (ID: {UserId}) outside working hours at {DateTime}. Permission claim: '{PermissionClaim}'", 
                        username, userId, DateTime.Now, permissionClaim);

                    context.Response.StatusCode = 403;
                    context.Response.ContentType = "application/json";
                    var errorResponse = new
                    {
                        errorMessage = "Actions are restricted outside working hours. Please try during working hours. Note: If you were just assigned the 'Work Outside Working Hours' permission, please log out and log back in to refresh your session.",
                        message = "Actions are restricted outside working hours. Please try during working hours. Note: If you were just assigned the 'Work Outside Working Hours' permission, please log out and log back in to refresh your session."
                    };
                    await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(errorResponse));
                    return;
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in WorkingHoursMiddleware");
                // On error, allow access to avoid blocking users
                await _next(context);
            }
        }

        private bool ShouldSkipWorkingHoursCheck(string path)
        {
            var skipPaths = new[]
            {
                "/api/users/login",
                "/api/users/logout",
                "/api/health",
                "/swagger",
                "/favicon.ico",
                "/resources",
                "/index.html",
                "/"
            };

            return skipPaths.Any(skipPath => path.StartsWith(skipPath));
        }
    }

    public static class WorkingHoursMiddlewareExtensions
    {
        public static IApplicationBuilder UseWorkingHours(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<WorkingHoursMiddleware>();
        }
    }
}
