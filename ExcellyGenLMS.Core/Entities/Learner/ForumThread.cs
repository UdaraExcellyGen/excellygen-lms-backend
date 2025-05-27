using ExcellyGenLMS.Core.Entities.Auth;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace ExcellyGenLMS.Core.Entities.Learner
{
    [Table("ForumThreads")]
    public class ForumThread
    {
        [Key]
        [Column("ThreadId")] // Database column name
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Tells EF Core this is an Identity column
        public int Id { get; set; } // C# property for PK is now int

        [Required]
        [StringLength(200)]
        [Column("Title")] // You'll add this column
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column("Content")]
        public string Content { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Column("Category")]
        public string Category { get; set; } = string.Empty;

        [StringLength(512)]
        [Column("ImageUrl")] // You'll add this column
        public string? ImageUrl { get; set; }

        [Required]
        [Column("Date")] // Your existing timestamp column
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // UpdatedAt was removed as per earlier request

        [Required]
        [Column("Creator")] // Your existing column for UserId (string)
        public string CreatorId { get; set; } = string.Empty; // This MUST remain string to match User.Id
        [ForeignKey("CreatorId")]
        public virtual User? Creator { get; set; }

        public virtual ICollection<ThreadComment> Comments { get; set; } = new List<ThreadComment>();
    }
}