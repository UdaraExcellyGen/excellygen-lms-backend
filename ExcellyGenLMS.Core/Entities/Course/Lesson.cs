using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExcellyGenLMS.Core.Entities.Course
{
    [Table("Lessons")]
    public class Lesson
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public required string LessonName { get; set; } // Represents Subtopic Title from frontend

        [Required] // Points required for each lesson
        public int LessonPoints { get; set; }

        public DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;

        [Required]
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!; // Ensure non-nullable relationship

        // Initialize collection to avoid null reference issues
        public virtual ICollection<CourseDocument> Documents { get; set; } = new List<CourseDocument>();
    }
}