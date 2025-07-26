using ExcellyGenLMS.Core.Enums;

namespace ExcellyGenLMS.Application.DTOs.Course
{
    // --- DTO for retrieving Course Document details ---
    public class CourseDocumentDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public DocumentType DocumentType { get; set; }
        public long FileSize { get; set; } // In bytes
        public required string FileUrl { get; set; } // Publicly accessible URL from file service
        public DateTime LastUpdatedDate { get; set; }
        public int LessonId { get; set; }
        public bool IsCompleted { get; set; }
    }

    // --- DTO for Upload Response (or use CourseDocumentDto) ---
    // Often, just returning the CourseDocumentDto after upload is sufficient.
    // If you needed more info specific to the upload action, you could create one:
    // public class UploadDocumentResponseDto : CourseDocumentDto
    // {
    //     public bool WasSuccessful { get; set; }
    //     public string? Message { get; set; }
    // }

    // Note: Actual file upload typically uses IFormFile directly in the controller/service method signature,
    // so a specific "CreateDocumentDto" containing the file itself isn't usually needed here.
}