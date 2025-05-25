// MCQQuestionOption.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ExcellyGenLMS.Core.Entities.Course;

namespace ExcellyGenLMS.Core.Entities.Course
{
    [Table("MCQQuestionOptions")]
    public class MCQQuestionOption
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("mcq_option_id")]
        public int McqOptionId { get; set; }

        [Required]
        [Column("quiz_bank_question_id")]
        public int QuizBankQuestionId { get; set; }
        [ForeignKey("QuizBankQuestionId")]
        public QuizBankQuestion? QuizBankQuestion { get; set; } // Changed to nullable

        [Required]
        [Column("option_text", TypeName = "TEXT")]
        public required string OptionText { get; set; } // Keep this required

        [Column("is_correct")]
        public bool IsCorrect { get; set; }
    }
}