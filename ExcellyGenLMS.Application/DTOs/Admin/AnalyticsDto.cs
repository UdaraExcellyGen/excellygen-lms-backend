// ExcellyGenLMS.Application/DTOs/Admin/AnalyticsDto.cs
using System.Collections.Generic;

namespace ExcellyGenLMS.Application.DTOs.Admin
{
    public class ChartDataDto
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
        public string? Color { get; set; }
    }

    public class EnrollmentAnalyticsDto
    {
        public List<ChartDataDto> EnrollmentData { get; set; } = new List<ChartDataDto>();
        public List<CourseCategoryDto> Categories { get; set; } = new List<CourseCategoryDto>();
    }

    public class CourseAvailabilityDto
    {
        public List<ChartDataDto> AvailabilityData { get; set; } = new List<ChartDataDto>();
    }

    public class UserDistributionDto
    {
        public List<ChartDataDto> DistributionData { get; set; } = new List<ChartDataDto>();
    }

    public class DashboardAnalyticsDto
    {
        public EnrollmentAnalyticsDto EnrollmentAnalytics { get; set; } = new EnrollmentAnalyticsDto();
        public CourseAvailabilityDto CourseAvailability { get; set; } = new CourseAvailabilityDto();
        public UserDistributionDto UserDistribution { get; set; } = new UserDistributionDto();
    }
}