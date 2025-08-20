using System.Security.Claims;

namespace Warehousing.Api.middlewares
{
    public class ValidateUserClaimsMiddleware
    {
        private readonly RequestDelegate _next;

        public ValidateUserClaimsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var user = context.User;
            if (user.Identity?.IsAuthenticated == true)
            {
                var nameClaim = user.FindFirst(ClaimTypes.Name);
                if (nameClaim == null || string.IsNullOrEmpty(nameClaim.Value))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Invalid token: Missing user identity.");
                    return;
                }
            }
            await _next(context);
        }
    }
}