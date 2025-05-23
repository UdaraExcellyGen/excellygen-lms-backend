using System.ComponentModel.DataAnnotations;

namespace ExcellyGenLMS.Application.DTOs.Course
{
    // --- DTO for retrieving Lesson details (including its documents) ---
    public class LessonDto
    {
        public int Id { get; set; }
        public required string LessonName { get; set; } // Corresponds to 'Subtopic' title
        public int LessonPoints { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public int CourseId { get; set; }
        public required List<CourseDocumentDto> Documents { get; set; } = new(); // List of Document DTOs
    }

    // --- DTO for creating a new Lesson (Subtopic) ---
    public class CreateLessonDto
    {
        [Required]
        public int CourseId { get; set; } // The course this lesson belongs to

        [Required(ErrorMessage = "Lesson (Subtopic) name is required.")]
        [MaxLength(200, ErrorMessage = "Lesson name cannot exceed 200 characters.")]
        public required string LessonName { get; set; }

        [Required(ErrorMessage = "Lesson points are required.")]
        [Range(1, 100, ErrorMessage = "Points must be between 1 and 100.")] // Adjust range as needed
        public int LessonPoints { get; set; }
    }

    // --- DTO for updating an existing Lesson (Subtopic) ---
    public class UpdateLessonDto
    {
        // Note: CourseId is usually not updatable for a lesson
        [Required(ErrorMessage = "Lesson (Subtopic) name is required.")]
        [MaxLength(200, ErrorMessage = "Lesson name cannot exceed 200 characters.")]
        public required string LessonName { get; set; }

        [Required(ErrorMessage = "Lesson points are required.")]
        [Range(1, 100, ErrorMessage = "Points must be between 1 and 100.")] // Adjust range as needed
        public int LessonPoints { get; set; }
    }
}