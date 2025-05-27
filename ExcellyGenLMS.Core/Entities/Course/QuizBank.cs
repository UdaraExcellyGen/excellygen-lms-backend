// QuizBank.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ExcellyGenLMS.Core.Entities.Course; // Add this using for correct namespace

namespace ExcellyGenLMS.Core.Entities.Course
{    // Corrected Namespace
    [Table("QuizBanks")] // Corrected Table name to "QuizBanks" (plural convention)
    public class QuizBank
    {
        [Key]
        [Column("quiz_bank_id")]
        public int QuizBankId { get; set; }

        [Column("quiz_bank_size")]
        public int QuizBankSize { get; set; }

        public ICollection<QuizBankQuestion> QuizBankQuestions { get; set; } = new List<QuizBankQuestion>(); // Navigation property to QuizBankQuestions, initialize to avoid null exceptions
        public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>(); // Navigation property to Quizzes, initialize to avoid null exceptions
    }
}