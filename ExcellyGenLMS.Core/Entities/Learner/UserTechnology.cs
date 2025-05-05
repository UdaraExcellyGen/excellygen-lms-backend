using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ExcellyGenLMS.Core.Entities.Auth;
using ExcellyGenLMS.Core.Entities.Admin;

namespace ExcellyGenLMS.Core.Entities.Learner
{
    public class UserTechnology
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [Required]
        public string TechnologyId { get; set; } = string.Empty;

        [ForeignKey("TechnologyId")]
        public virtual Technology Technology { get; set; } = null!;

        public DateTime AddedDate { get; set; } = DateTime.UtcNow;
    }
}