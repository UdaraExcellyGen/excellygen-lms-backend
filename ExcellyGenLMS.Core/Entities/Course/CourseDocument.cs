// CourseDocument.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExcellyGenLMS.Core.Entities.Course
{
    public enum DocumentType
    {
        PDF,
        Word
    }

    [Table("CourseDocuments")]
    public class CourseDocument
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public required string Name { get; set; }

        [Required]
        public DocumentType DocumentType { get; set; }

        public DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(2048)]
        public required string FilePath { get; set; }

        public long FileSize { get; set; }
        
        // Foreign key for Lesson
        public int LessonId { get; set; }
        
        // Navigation property to Lesson
        [ForeignKey("LessonId")]
        public Lesson Lesson { get; set; } = null!;
    }
}