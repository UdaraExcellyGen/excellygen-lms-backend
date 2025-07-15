using ExcellyGenLMS.Core.Enums;
using Microsoft.AspNetCore.Http; // Required for IFormFile
using System.ComponentModel.DataAnnotations;

namespace ExcellyGenLMS.Application.DTOs.Course
{
    // --- DTO for retrieving Course Details ---
    public class CourseDto
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public int? CalculatedCoursePoints { get; set; } // Sum of lesson points, calculated by service
        public int EstimatedTime { get; set; } // In Hours
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public CourseStatus Status { get; set; }
        public bool IsInactive { get; set; }
        public string? ThumbnailUrl { get; set; } // Publicly accessible URL from file service
        public required CategoryDto Category { get; set; } // Nested DTO for Category info
        public required UserBasicDto Creator { get; set; }   // Nested DTO for basic Creator info
        public required List<TechnologyDto> Technologies { get; set; } = new(); // List of Tech DTOs
        public required List<LessonDto> Lessons { get; set; } = new();        // List of Lesson DTOs
    }

    // --- DTO for creating a new Course (Step 1 - Basic Details) ---
    // This DTO will be bound using [FromForm] in the controller due to IFormFile
    public class CreateCourseDto
    {
        [Required(ErrorMessage = "Course Title is required.")]
        [MaxLength(200, ErrorMessage = "Course Title cannot exceed 200 characters.")]
        public required string Title { get; set; }

        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")] // Example MaxLength
        public string? Description { get; set; }

        [Required(ErrorMessage = "Estimated time in hours is required.")]
        [Range(1, 1000, ErrorMessage = "Estimated time must be between 1 and 1000 hours.")]
        public int EstimatedTime { get; set; } // In Hours

        [Required(ErrorMessage = "Course Category is required.")]
        public string CategoryId { get; set; } = string.Empty; // ID of the selected category

        [Required(ErrorMessage = "At least one technology must be selected.")]
        [MinLength(1, ErrorMessage = "Please select at least one technology.")]
        public List<string> TechnologyIds { get; set; } = new(); // List of Technology IDs (strings)

        // CreatorId will be set by the service based on the authenticated user
        // CoursePoints is calculated later based on lessons
        // Status is set to Draft initially by the service

        public IFormFile? ThumbnailImage { get; set; } // The uploaded thumbnail file (optional)
    }

    // --- DTO for retrieving basic User Info (Used within CourseDto) ---
    public class UserBasicDto
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        // Add other fields like Email if needed in the future
    }

    // --- DTO for retrieving basic Category Info (Used within CourseDto) ---
    public class CategoryDto
    {
        public required string Id { get; set; }
        public required string Title { get; set; }
    }

    // --- DTO for retrieving basic Technology Info (Used within CourseDto) ---
    public class TechnologyDto
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
    }
}