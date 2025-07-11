using System.ComponentModel.DataAnnotations;

namespace ExcellyGenLMS.Application.DTOs.Admin
{
    /// <summary>
    /// DTO for Admins or Coordinators updating specific fields of a course.
    /// This is the object received from the frontend when 'Save Changes' is clicked.
    /// </summary>
    public class UpdateCourseAdminDto
    {
        [Required(ErrorMessage = "Course Title is required.")]
        [MaxLength(200, ErrorMessage = "Course Title cannot exceed 200 characters.")]
        public required string Title { get; set; }

        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
        public string? Description { get; set; }

        /// <summary>
        /// The ID of the category to associate the course with.
        /// This is the crucial field that was missing.
        /// </summary>
        [Required(ErrorMessage = "Category ID is required.")]
        public string? CategoryId { get; set; }

        // Note: You can add other properties here if they need to be editable,
        // such as TechnologyIds, EstimatedTime, etc.
    }
}