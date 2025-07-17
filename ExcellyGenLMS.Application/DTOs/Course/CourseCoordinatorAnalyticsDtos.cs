// ExcellyGenLMS.Application/DTOs/Course/CourseCoordinatorAnalyticsDtos.cs
using System.Collections.Generic;

namespace ExcellyGenLMS.Application.DTOs.Course
{
    // Original DTOs (keeping for backward compatibility)
    public class CourseEnrollmentAnalyticsDto
    {
        public required string course { get; set; }
        public int count { get; set; }
    }

    public class MarkRangeDataDto
    {
        public required string range { get; set; }
        public int count { get; set; }
    }

    public class CoordinatorCourseDto
    {
        public int CourseId { get; set; }
        public required string CourseTitle { get; set; }
    }

    public class CourseQuizDto
    {
        public int QuizId { get; set; }
        public required string QuizTitle { get; set; }
    }

    // Enhanced DTOs for new functionality

    /// <summary>
    /// Enhanced enrollment data with category and status information
    /// </summary>
    public class EnrollmentAnalyticsDto
    {
        public int CourseId { get; set; }
        public string Course { get; set; } = string.Empty;
        public string ShortCourseName { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty; // String to match your entity
        public string CategoryName { get; set; } = string.Empty;
        public int TotalEnrollments { get; set; }
        public int OngoingEnrollments { get; set; }
        public int CompletedEnrollments { get; set; }
        public string CoordinatorId { get; set; } = string.Empty;
        public string CoordinatorName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Course category for filtering
    /// </summary>
    public class CourseCategoryAnalyticsDto
    {
        public string Id { get; set; } = string.Empty; // String to match your entity
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TotalCourses { get; set; }
        public int TotalEnrollments { get; set; }
    }

    /// <summary>
    /// Enhanced coordinator course data
    /// </summary>
    public class CoordinatorCourseAnalyticsDto
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string ShortTitle { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty; // String to match your entity
        public string CategoryName { get; set; } = string.Empty;
        public int TotalEnrollments { get; set; }
        public int OngoingEnrollments { get; set; }
        public int CompletedEnrollments { get; set; }
        public bool IsCreatedByCurrentCoordinator { get; set; }
    }

    /// <summary>
    /// Quiz data with enhanced information
    /// </summary>
    public class CourseQuizAnalyticsDto
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public int TotalAttempts { get; set; }
        public decimal AverageScore { get; set; }
        public bool IsCreatedByCurrentCoordinator { get; set; }
    }

    /// <summary>
    /// Enhanced mark range data with better intervals
    /// </summary>
    public class MarkRangeAnalyticsDto
    {
        public string Range { get; set; } = string.Empty; // e.g., "0-20", "21-40"
        public int MinMark { get; set; }
        public int MaxMark { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    /// <summary>
    /// Response for enrollment analytics endpoint
    /// </summary>
    public class EnrollmentAnalyticsResponse
    {
        public List<EnrollmentAnalyticsDto> Enrollments { get; set; } = new();
        public List<CourseCategoryAnalyticsDto> Categories { get; set; } = new();
        public EnrollmentStatsDto TotalStats { get; set; } = new();
    }

    /// <summary>
    /// Total enrollment statistics
    /// </summary>
    public class EnrollmentStatsDto
    {
        public int TotalCourses { get; set; }
        public int TotalEnrollments { get; set; }
        public int TotalOngoing { get; set; }
        public int TotalCompleted { get; set; }
    }

    /// <summary>
    /// Response for quiz performance endpoint
    /// </summary>
    public class QuizPerformanceAnalyticsResponse
    {
        public List<MarkRangeAnalyticsDto> PerformanceData { get; set; } = new();
        public QuizStatsDto QuizStats { get; set; } = new();
    }

    /// <summary>
    /// Quiz statistics
    /// </summary>
    public class QuizStatsDto
    {
        public int TotalAttempts { get; set; }
        public decimal AverageScore { get; set; }
        public decimal PassRate { get; set; }
    }

    /// <summary>
    /// Enrollment status enumeration
    /// </summary>
    public enum EnrollmentStatusFilter
    {
        All,
        Ongoing,
        Completed
    }
}