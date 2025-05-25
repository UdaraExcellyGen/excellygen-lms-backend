// ExcellyGenLMS.Application/DTOs/CommonStatsDto.cs
// This DTO defines common overall LMS statistics for various dashboards (e.g., Learner/Admin)
// It is separate from Admin specific DashboardStatsDto.
using System;

namespace ExcellyGenLMS.Application.DTOs
{
    public class OverallLmsStatsDto
    {
        public int TotalCategories { get; set; }
        public int TotalPublishedCourses { get; set; }
        public int TotalActiveLearners { get; set; }
        public int TotalActiveCoordinators { get; set; }
        public int TotalProjectManagers { get; set; }
        public string AverageCourseDurationOverall { get; set; } = "N/A";
    }
}