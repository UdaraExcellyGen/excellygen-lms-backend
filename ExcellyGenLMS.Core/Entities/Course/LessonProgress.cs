// ExcellyGenLMS.Core/Entities/Course/LessonProgress.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ExcellyGenLMS.Core.Entities.Auth; // For User entity

namespace ExcellyGenLMS.Core.Entities.Course
{
    [Table("LessonProgress")]
    public class LessonProgress
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int LessonId { get; set; }

        [Required]
        public bool IsCompleted { get; set; } = false;

        public DateTime? CompletionDate { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("LessonId")]
        public virtual Lesson Lesson { get; set; } = null!;
    }
}