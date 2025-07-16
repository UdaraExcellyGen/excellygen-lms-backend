using ExcellyGenLMS.Core.Entities.Auth;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExcellyGenLMS.Core.Entities.Learner
{
    public class UserActivityLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = null!; // Fixed: Initialized to satisfy compiler

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!; // Fixed: Initialized to satisfy compiler

        [Required]
        public DateTime ActivityTimestamp { get; set; } = DateTime.UtcNow;
    }
}