using Microsoft.Extensions.DependencyInjection;
using ExcellyGenLMS.Core.Interfaces.Repositories.Auth;
using ExcellyGenLMS.Infrastructure.Data.Repositories.Auth;
using ExcellyGenLMS.Application.Interfaces.Admin;
using ExcellyGenLMS.Application.Services.Admin;
// Add these new using statements for Firebase Storage
using ExcellyGenLMS.Core.Interfaces.Infrastructure;
using ExcellyGenLMS.Infrastructure.Services.Storage;

namespace ExcellyGenLMS.API.Extensions
{
    public static class ServiceRegistration
    {
        public static void RegisterUserManagementServices(this IServiceCollection services)
        {
            // Register User Management services
            services.AddScoped<IUserManagementService, UserManagementService>();
        }

        // Add this new method for Firebase Storage registration
        public static void RegisterStorageServices(this IServiceCollection services)
        {
            // Register Firebase Storage Service
            services.AddScoped<IFileStorageService, FirebaseStorageService>();
        }
    }
}