using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;

namespace ExcellyGenLMS.API.Middleware
{
    public class RoleAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RoleAuthorizationMiddleware> _logger;

        public RoleAuthorizationMiddleware(RequestDelegate next, ILogger<RoleAuthorizationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip authorization check for auth endpoints and non-API routes
            if (!context.Request.Path.StartsWithSegments("/api") ||
                context.Request.Path.StartsWithSegments("/api/auth") ||
                context.Request.Method == "OPTIONS")
            {
                await _next(context);
                return;
            }

            // Get the current role from the JWT claims
            var currentRole = context.User.Claims.FirstOrDefault(c => c.Type == "CurrentRole")?.Value;

            if (string.IsNullOrEmpty(currentRole))
            {
                _logger.LogWarning("Authorization denied: No current role specified in token");
                await HandleUnauthorizedResponse(context, "No role specified. Please login and select a role.");
                return;
            }

            // Check if the route requires a specific role
            var path = context.Request.Path.Value;
            if (path == null)
            {
                await _next(context);
                return;
            }

            path = path.ToLowerInvariant();

            // Check admin routes
            if (path.Contains("/api/admin") && currentRole != "Admin")
            {
                _logger.LogWarning($"Authorization denied: User with role {currentRole} tried to access admin route");
                await HandleUnauthorizedResponse(context, "This endpoint requires Admin role.");
                return;
            }

            // Check coordinator routes
            if (path.Contains("/api/coordinator") && currentRole != "CourseCoordinator")
            {
                _logger.LogWarning($"Authorization denied: User with role {currentRole} tried to access coordinator route");
                await HandleUnauthorizedResponse(context, "This endpoint requires CourseCoordinator role.");
                return;
            }

            // Check project manager routes
            if ((path.Contains("/api/projectmanager") || path.Contains("/api/project-manager")) && currentRole != "ProjectManager")
            {
                _logger.LogWarning($"Authorization denied: User with role {currentRole} tried to access project manager route");
                await HandleUnauthorizedResponse(context, "This endpoint requires ProjectManager role.");
                return;
            }

            // Check learner routes
            if (path.Contains("/api/learner") && currentRole != "Learner")
            {
                _logger.LogWarning($"Authorization denied: User with role {currentRole} tried to access learner route");
                await HandleUnauthorizedResponse(context, "This endpoint requires Learner role.");
                return;
            }

            // Continue with the request
            await _next(context);
        }

        private static async Task HandleUnauthorizedResponse(HttpContext context, string message)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            context.Response.ContentType = "application/json";

            var response = JsonSerializer.Serialize(new
            {
                StatusCode = 403,
                Message = message
            });

            await context.Response.WriteAsync(response);
        }
    }

    // Extension method for middleware registration
    public static class RoleAuthorizationMiddlewareExtensions
    {
        public static IApplicationBuilder UseRoleAuthorization(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RoleAuthorizationMiddleware>();
        }
    }
}