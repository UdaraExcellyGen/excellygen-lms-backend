using System.Threading.Tasks;
using ExcellyGenLMS.Application.DTOs.Admin;

namespace ExcellyGenLMS.Application.Interfaces.Admin
{
    public interface IAnalyticsService
    {
        Task<KpiSummaryDto> GetKpiSummaryAsync();
        Task<EnrollmentKpiDto> GetEnrollmentKpiAsync();
        Task<EnrollmentAnalyticsDto> GetEnrollmentAnalyticsAsync(string? categoryId = null);
        Task<CourseAvailabilityDto> GetCourseAvailabilityAnalyticsAsync();
        Task<UserDistributionDto> GetUserDistributionAnalyticsAsync();
    }
}