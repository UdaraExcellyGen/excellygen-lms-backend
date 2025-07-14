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
        public required string UserId { get; set; }

        [Required]
        public required string BadgeId { get; set; }

        // FIX: This property is required by the business logic.
        public bool IsClaimed { get; set; }

        // FIX: This property is required by the business logic and must be nullable.
        public DateTime? DateEarned { get; set; }

        // --- Navigation Properties ---
        [ForeignKey("UserId")]
        public User? User { get; set; }

        [ForeignKey("BadgeId")]
        public Badge? Badge { get; set; }
    }
}