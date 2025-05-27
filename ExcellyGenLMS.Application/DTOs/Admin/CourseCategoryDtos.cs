// ExcellyGenLMS.Application/DTOs/Admin/CourseCategoryDtos.cs
using System.ComponentModel.DataAnnotations; // Needed for [Required], [StringLength] attributes

namespace ExcellyGenLMS.Application.DTOs.Admin
{
    // DTO for retrieving Course Category details
    public class CourseCategoryDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty; // Name of the icon (e.g., "code", "palette")
        public string Status { get; set; } = string.Empty; // e.g., "active", "inactive"
        public int TotalCourses { get; set; } // Count of courses within this category
        public int ActiveLearnersCount { get; set; } // Count of unique active learners enrolled in courses of this category
        public string AvgDuration { get; set; } = "N/A"; // Average estimated duration of courses in this category (e.g., "6 months", "20 hours")
    }

    // DTO for creating a new Course Category
    public class CreateCourseCategoryDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Icon name cannot exceed 100 characters.")]
        public string Icon { get; set; } = string.Empty;
    }

    // DTO for updating an existing Course Category
    public class UpdateCourseCategoryDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Icon name cannot exceed 100 characters.")]
        public string Icon { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required.")]
        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters.")]
        public string Status { get; set; } = string.Empty;
    }
}