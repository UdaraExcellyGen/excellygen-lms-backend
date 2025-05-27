using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ExcellyGenLMS.Core.Enums; // Ensure this using is present

namespace ExcellyGenLMS.Core.Entities.Course
{
    [Table("CourseDocuments")]
    public class CourseDocument
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)] // Increased practical length
        public required string Name { get; set; }

        [Required]
        public DocumentType DocumentType { get; set; }

        public DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(1024)] // Increased length for paths/URLs
        public required string FilePath { get; set; } // Store relative path or URL

        public long FileSize { get; set; } // In bytes

        [Required]
        public int LessonId { get; set; }

        [ForeignKey("LessonId")]
        public virtual Lesson Lesson { get; set; } = null!; // Ensure non-nullable relationship
    }
}