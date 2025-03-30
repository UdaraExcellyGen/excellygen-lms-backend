// Quiz.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ExcellyGenLMS.Core.Entities.Course;

namespace ExcellyGenLMS.Core.Entities.Course
{
    [Table("Quizzes")]
    public class Quiz
    {
        [Key]
        [Column("quiz_id")]
        public int QuizId { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("quiz_title")]
        public required string QuizTitle { get; set; } // Added required

        [Column("time_limit_minutes")]
        public int TimeLimitMinutes { get; set; }

        [Column("total_marks")]
        public int TotalMarks { get; set; }

        [Column("quiz_size")]
        public int QuizSize { get; set; }

        [Column("quiz_bank_id")]
        public int QuizBankId { get; set; }
        [ForeignKey("QuizBankId")]
        public required QuizBank QuizBank { get; set; } // Navigation property - Added required

        [Column("lesson_id")]
        public int LessonId { get; set; }
        [ForeignKey("LessonId")]
        public required Lesson Lesson { get; set; } // Navigation property - Added required
    }
}