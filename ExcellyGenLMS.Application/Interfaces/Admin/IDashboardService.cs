using ExcellyGenLMS.Application.DTOs.Admin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Interfaces.Admin
{
    public interface IDashboardService
    {
        /// <summary>
        /// Gets statistics for the admin dashboard including course categories, users, and technologies
        /// </summary>
        /// <returns>Dashboard statistics</returns>
        Task<DashboardStatsDto> GetDashboardStatsAsync();

        /// <summary>
        /// Gets recent notifications for the admin dashboard
        /// </summary>
        /// <returns>List of notifications</returns>
        Task<List<NotificationDto>> GetDashboardNotificationsAsync();
    }
}