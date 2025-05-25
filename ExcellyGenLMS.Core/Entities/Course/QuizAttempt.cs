// QuizAttempt.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ExcellyGenLMS.Core.Entities.Auth;

namespace ExcellyGenLMS.Core.Entities.Course
{
    [Table("QuizAttempts")]
    public class QuizAttempt
    {
        [Key]
        [Column("quiz_attempt_id")]
        public int QuizAttemptId { get; set; }

        [Required]
        [Column("quiz_id")]
        public int QuizId { get; set; }
        [ForeignKey("QuizId")]
        public Quiz? Quiz { get; set; } // Changed to nullable

        [Required]
        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public User? User { get; set; } // Changed to nullable

        [Column("start_time")]
        public DateTime StartTime { get; set; } = DateTime.UtcNow;

        [Column("completion_time")]
        public DateTime? CompletionTime { get; set; }

        [Column("score")]
        public int? Score { get; set; }

        [Column("is_completed")]
        public bool IsCompleted { get; set; } = false;

        [Column("total_questions")]
        public int TotalQuestions { get; set; }

        [Column("correct_answers")]
        public int CorrectAnswers { get; set; } = 0;

        public ICollection<QuizAttemptAnswer> Answers { get; set; } = new List<QuizAttemptAnswer>();
    }
}