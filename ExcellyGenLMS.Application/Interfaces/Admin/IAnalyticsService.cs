// ExcellyGenLMS.Application/Interfaces/Admin/IAnalyticsService.cs
using System.Threading.Tasks;
using ExcellyGenLMS.Application.DTOs.Admin;

namespace ExcellyGenLMS.Application.Interfaces.Admin
{
    public interface IAnalyticsService
    {
        Task<DashboardAnalyticsDto> GetDashboardAnalyticsAsync(string? categoryId = null);
        Task<EnrollmentAnalyticsDto> GetEnrollmentAnalyticsAsync(string? categoryId = null);
        Task<CourseAvailabilityDto> GetCourseAvailabilityAnalyticsAsync();
        Task<UserDistributionDto> GetUserDistributionAnalyticsAsync();
    }
}