// Lesson.cs
using System;
using System.Collections.Generic;
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
        public required string LessonName { get; set; }

        [Required]
        public int LessonPoints { get; set; }

        public DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;

        // Foreign key for Course
        public int CourseId { get; set; }
        
        // Navigation property to Course
        [ForeignKey("CourseId")]
        public Course Course { get; set; } = null!;

        // Navigation properties
        //public ICollection<Quiz>? Quizzes { get; set; }
        public ICollection<CourseDocument>? Documents { get; set; }
    }
}