// ExcellyGenLMS.Application/DTOs/Course/CourseCoordinatorDtos.cs
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ExcellyGenLMS.Application.DTOs.Course // Or ExcellyGenLMS.Application.DTOs.CourseCoordinator
{
    /// <summary>
    /// DTO used by Course Coordinators to update existing course details.
    /// This will likely require [FromForm] binding if ThumbnailImage is included.
    /// </summary>
    public class UpdateCourseCoordinatorDto
    {
        [Required(ErrorMessage = "Course Title is required.")]
        [MaxLength(200, ErrorMessage = "Course Title cannot exceed 200 characters.")]
        public required string Title { get; set; }

        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Estimated time in hours is required.")]
        [Range(1, 1000, ErrorMessage = "Estimated time must be between 1 and 1000 hours.")]
        public int EstimatedTime { get; set; } // In Hours

        [Required(ErrorMessage = "Course Category is required.")]
        public string CategoryId { get; set; } = string.Empty; // ID of the selected category

        [Required(ErrorMessage = "At least one technology must be selected.")]
        [MinLength(1, ErrorMessage = "Please select at least one technology.")]
        public List<string> TechnologyIds { get; set; } = new(); // List of Technology IDs (strings)

        // Coordinator might want to update the thumbnail after initial creation
        public IFormFile? ThumbnailImage { get; set; } // Make this nullable if update is optional
    }
}