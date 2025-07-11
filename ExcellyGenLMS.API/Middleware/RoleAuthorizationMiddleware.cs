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
            // Skip authorization check for auth endpoints, non-API routes, and OPTIONS requests
            if (!context.Request.Path.StartsWithSegments("/api") ||
                context.Request.Path.StartsWithSegments("/api/auth") ||
                context.Request.Method == "OPTIONS")
            {
                await _next(context);
                return;
            }

            // Get the current role from the JWT claims
            var currentRole = context.User.Claims.FirstOrDefault(c => c.Type == "CurrentRole")?.Value;
            var allUserRoles = context.User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

            if (string.IsNullOrEmpty(currentRole))
            {
                _logger.LogWarning("Authorization denied: No current role specified in token");
                await HandleUnauthorizedResponse(context, "No role specified. Please login and select a role.");
                return;
            }

            var path = context.Request.Path.Value;
            if (path == null)
            {
                await _next(context);
                return;
            }

            path = path.ToLowerInvariant();

            // OPTIMIZATION: Check if user has access to this route
            if (!HasAccessToRoute(path, currentRole, allUserRoles))
            {
                _logger.LogWarning($"Authorization denied: User with role {currentRole} tried to access {path}");
                await HandleUnauthorizedResponse(context, $"This endpoint requires appropriate role access.");
                return;
            }

            await _next(context);
        }

        // OPTIMIZATION: Centralized route access logic to prevent duplicate warnings
        private static bool HasAccessToRoute(string path, string currentRole, List<string> allUserRoles)
        {
            // Admin routes - Only Admin can access
            if (path.Contains("/api/admin"))
            {
                return currentRole == "Admin" || allUserRoles.Contains("Admin");
            }

            // Coordinator routes - Only CourseCoordinator can access
            if (path.Contains("/api/coordinator"))
            {
                return currentRole == "CourseCoordinator" || allUserRoles.Contains("CourseCoordinator");
            }

            // Project Manager routes - Only ProjectManager can access
            if (path.Contains("/api/projectmanager") || path.Contains("/api/project-manager"))
            {
                return currentRole == "ProjectManager" || allUserRoles.Contains("ProjectManager");
            }

            // Learner routes - Only Learner can access
            if (path.Contains("/api/learner"))
            {
                return currentRole == "Learner" || allUserRoles.Contains("Learner");
            }

            // OPTIMIZATION: Allow shared/common routes that multiple roles can access
            if (IsSharedRoute(path))
            {
                return true;
            }

            // Default: Allow access for unspecified routes
            return true;
        }

        // OPTIMIZATION: Define shared routes that multiple roles can access
        private static bool IsSharedRoute(string path)
        {
            var sharedRoutes = new[]
            {
                "/api/coursecategories", // Both Admin and Learner can access (with different permissions)
                "/api/common/",
                "/api/files/",
                "/api/notifications/",
                "/api/dashboard/",
                "/api/profile/"
            };

            return sharedRoutes.Any(route => path.Contains(route));
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