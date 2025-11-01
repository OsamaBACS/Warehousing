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

                // Check working hours for non-admin users
                var isWithinWorkingHours = await workingHoursRepo.IsWithinWorkingHoursAsync(DateTime.Now);
                
                if (!isWithinWorkingHours)
                {
                    _logger.LogWarning("Access denied for user {UserId} outside working hours at {DateTime}", 
                        context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, DateTime.Now);

                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsync("Access denied. You can only access the system during working hours.");
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
