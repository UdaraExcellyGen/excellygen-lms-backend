using System;
using System.ComponentModel.DataAnnotations;

namespace ExcellyGenLMS.Core.Entities.Learner
{
    public class ForumThread
    {
        [Key]
        public int ThreadId { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.UtcNow;

        [Required]
        public string Creator { get; set; } = string.Empty;
    }
}