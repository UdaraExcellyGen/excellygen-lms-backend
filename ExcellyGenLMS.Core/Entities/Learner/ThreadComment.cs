using ExcellyGenLMS.Core.Entities.Auth;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace ExcellyGenLMS.Core.Entities.Learner
{
    [Table("ThreadComments")]
    public class ThreadComment
    {
        [Key]
        [Column("Id")] // Database column name
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } // C# property for PK is now int

        [Required]
        [Column("ThreadId")] // Database column name for FK
        public int ThreadId { get; set; } // FK to ForumThread.Id is now int
        [ForeignKey("ThreadId")]
        public virtual ForumThread? Thread { get; set; }

        [Required]
        [Column("Content")]
        public string Content { get; set; } = string.Empty;

        [Required]
        [Column("Date")] // Your existing timestamp column
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // UpdatedAt was removed

        [Required]
        [Column("Commentor")] // Your existing column for UserId (string)
        public string CommentorId { get; set; } = string.Empty; // This MUST remain string
        [ForeignKey("CommentorId")]
        public virtual User? Commentor { get; set; }

        public virtual ICollection<ThreadComReply> Replies { get; set; } = new List<ThreadComReply>();
    }
}