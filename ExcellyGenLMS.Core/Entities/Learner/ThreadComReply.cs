using ExcellyGenLMS.Core.Entities.Auth;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExcellyGenLMS.Core.Entities.Learner
{
    [Table("ThreadComReplies")]
    public class ThreadComReply
    {
        [Key]
        [Column("Id")] // Database column name
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } // C# property for PK is now int

        // Your original table migration "20250329153259_AddThreadComReplyTable" also had a "ThreadId" FK directly here.
        // If that ThreadId column still exists in your actual "ThreadComReplies" table AND you want to map it:
        // [Column("ThreadId")]
        // public int RedundantThreadId { get; set; } // Would also need to be int if ForumThread.Id is int
        // [ForeignKey("RedundantThreadId")]
        // public virtual ForumThread? DirectThread {get; set;}


        [Required]
        [Column("CommentId")] // Database column name for FK
        public int CommentId { get; set; } // FK to ThreadComment.Id is now int
        [ForeignKey("CommentId")]
        public virtual ThreadComment? Comment { get; set; }

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
    }
}