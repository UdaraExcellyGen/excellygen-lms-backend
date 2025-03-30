using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExcellyGenLMS.Core.Entities.Learner
{
    public class ThreadComment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ThreadId { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.UtcNow;

        [Required]
        public string Commentor { get; set; } = string.Empty;

        [ForeignKey("ThreadId")]
        public virtual ForumThread? Thread { get; set; }
    }
}