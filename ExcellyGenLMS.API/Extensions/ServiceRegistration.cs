using Microsoft.Extensions.DependencyInjection;
using ExcellyGenLMS.Core.Interfaces.Repositories.Auth;
using ExcellyGenLMS.Infrastructure.Data.Repositories.Auth;
using ExcellyGenLMS.Application.Interfaces.Admin;
using ExcellyGenLMS.Application.Services.Admin;

namespace ExcellyGenLMS.API.Extensions
{
    public static class ServiceRegistration
    {
        public static void RegisterUserManagementServices(this IServiceCollection services)
        {
            // Register User Management services
            services.AddScoped<IUserManagementService, UserManagementService>();
        }
    }
}