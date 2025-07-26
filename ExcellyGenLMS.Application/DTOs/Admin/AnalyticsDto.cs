using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ExcellyGenLMS.Application.DTOs.Admin
{
    public class ChartDataDto
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
        public string? Color { get; set; }
    }

    public class EnrollmentChartItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int InProgress { get; set; }
        public int Completed { get; set; }
    }

    public class EnrollmentAnalyticsDto
    {
        public List<EnrollmentChartItemDto> EnrollmentData { get; set; } = new List<EnrollmentChartItemDto>();
        public List<CourseCategoryDto> Categories { get; set; } = new List<CourseCategoryDto>();
    }

    public class EnrollmentKpiDto
    {
        public string? MostPopularCategoryName { get; set; }
        public int MostPopularCategoryEnrollments { get; set; }
        public string? MostPopularCourseName { get; set; }
        public int MostPopularCourseEnrollments { get; set; }
        
        public string? MostCompletedCourseName { get; set; }
        public int MostCompletedCourseCount { get; set; }
    }

    public class KpiSummaryDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalCourses { get; set; }
        public int TotalEnrollments { get; set; }
        public int CompletedEnrollments { get; set; }
        
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public double CompletionRate => TotalEnrollments > 0 ? (double)CompletedEnrollments / TotalEnrollments * 100 : 0;
    }

    public class CourseAvailabilityDto
    {
        public List<ChartDataDto> AvailabilityData { get; set; } = new List<ChartDataDto>();
    }

    public class UserDistributionItemDto
    {
        public string Role { get; set; } = string.Empty;
        public int Active { get; set; }
        public int Inactive { get; set; }
    }

    public class UserDistributionDto
    {
        public List<UserDistributionItemDto> DistributionData { get; set; } = new List<UserDistributionItemDto>();
    }
}