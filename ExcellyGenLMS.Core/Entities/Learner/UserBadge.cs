using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ExcellyGenLMS.Core.Entities.Auth;

namespace ExcellyGenLMS.Core.Entities.Learner
{
    public class UserBadge
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [Required]
        public string BadgeId { get; set; } = string.Empty;

        [ForeignKey("BadgeId")]
        public virtual Badge Badge { get; set; } = null!;

        public DateTime EarnedDate { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;
    }
}