// File: ExcellyGenLMS.Core/Entities/ProjectManager/PMNotification.cs

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ExcellyGenLMS.Core.Entities.Auth;

namespace ExcellyGenLMS.Core.Entities.ProjectManager
{
    public class PMNotification
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(255)]
        public string Message { get; set; } = string.Empty;

        [Required]
        public string Type { get; set; } = string.Empty; // Assignment, Deadline, Update, etc.

        public string? ProjectId { get; set; }

        [ForeignKey("ProjectId")]
        public virtual PMProject? Project { get; set; }

        public string? RecipientId { get; set; }

        [ForeignKey("RecipientId")]
        public virtual User? Recipient { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}