using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ExcellyGenLMS.Core.Entities.Auth;

namespace ExcellyGenLMS.Core.Entities.Course
{
    [Table("DocumentProgress")]
    public class DocumentProgress
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int DocumentId { get; set; }

        [Required]
        public bool IsCompleted { get; set; } = false;

        public DateTime? CompletionDate { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("DocumentId")]
        public virtual CourseDocument Document { get; set; } = null!;
    }
}