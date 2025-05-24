using System.ComponentModel.DataAnnotations; // For validation attributes

namespace ExcellyGenLMS.Application.DTOs.Admin
{
    /// <summary>
    /// DTO for Admins updating specific fields of a course.
    /// </summary>
    public class UpdateCourseAdminDto
    {
        [Required(ErrorMessage = "Course Title is required.")]
        [MaxLength(200, ErrorMessage = "Course Title cannot exceed 200 characters.")]
        public required string Title { get; set; } // As used in CourseAdminService

        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
        public string? Description { get; set; } // As used in CourseAdminService

        // Add other fields here if Admins are allowed to update them
        // e.g., public string? CategoryId { get; set; }
    }
}