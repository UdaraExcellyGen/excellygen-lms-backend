using System;

namespace ExcellyGenLMS.Application.DTOs.Course
{
    /// <summary>
    /// Data transfer object for course enrollment information
    /// </summary>
    public class EnrollmentDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
    }

    /// <summary>
    /// Data transfer object for creating a new course enrollment
    /// </summary>
    public class CreateEnrollmentDto
    {
        public string UserId { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string Status { get; set; } = "active";
    }

    /// <summary>
    /// Data transfer object for updating an existing course enrollment
    /// </summary>
    public class UpdateEnrollmentDto
    {
        public string Status { get; set; } = string.Empty;
    }
}