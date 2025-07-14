// ExcellyGenLMS.Application/DTOs/Course/CourseCoordinatorAnalyticsDto.cs
using System.Collections.Generic;

namespace ExcellyGenLMS.Application.DTOs.Course
{
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
}