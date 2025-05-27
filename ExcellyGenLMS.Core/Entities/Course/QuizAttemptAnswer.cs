// QuizAttemptAnswer.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExcellyGenLMS.Core.Entities.Course
{
    [Table("QuizAttemptAnswers")]
    public class QuizAttemptAnswer
    {
        [Key]
        [Column("quiz_attempt_answer_id")]
        public int QuizAttemptAnswerId { get; set; }

        [Required]
        [Column("quiz_attempt_id")]
        public int QuizAttemptId { get; set; }
        [ForeignKey("QuizAttemptId")]
        public QuizAttempt? QuizAttempt { get; set; } // Changed to nullable

        [Required]
        [Column("quiz_bank_question_id")]
        public int QuizBankQuestionId { get; set; }
        [ForeignKey("QuizBankQuestionId")]
        public QuizBankQuestion? Question { get; set; } // Changed to nullable

        [Column("selected_option_id")]
        public int? SelectedOptionId { get; set; }
        [ForeignKey("SelectedOptionId")]
        public MCQQuestionOption? SelectedOption { get; set; }

        [Column("is_correct")]
        public bool IsCorrect { get; set; }
    }
}