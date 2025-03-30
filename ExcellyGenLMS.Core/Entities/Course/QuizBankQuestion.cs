// QuizBankQuestion.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ExcellyGenLMS.Core.Entities.Course;

namespace ExcellyGenLMS.Core.Entities.Course
{
    [Table("QuizBankQuestions")]
    public class QuizBankQuestion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("quiz_bank_question_id")]
        public int QuizBankQuestionId { get; set; }

        [Required]
        [Column("quiz_bank_id")]
        public int QuizBankId { get; set; }
        [ForeignKey("QuizBankId")]
        public required QuizBank QuizBank { get; set; } // Navigation property - Added required

        [Required]
        [Column("question_content", TypeName = "TEXT")]
        public required string QuestionContent { get; set; } // Added required

        [Column("question_type")]
        [MaxLength(50)]
        public string QuestionType { get; set; } = "mcq";

        [Column("question_bank_order")]
        public int? QuestionBankOrder { get; set; }

        public ICollection<MCQQuestionOption> MCQQuestionOptions { get; set; } = new List<MCQQuestionOption>();
    }
}