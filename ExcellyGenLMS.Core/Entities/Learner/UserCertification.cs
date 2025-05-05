using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ExcellyGenLMS.Core.Entities.Auth;

namespace ExcellyGenLMS.Core.Entities.Learner
{
    public class UserCertification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [Required]
        public string CertificationId { get; set; } = string.Empty;

        [ForeignKey("CertificationId")]
        public virtual Certification Certification { get; set; } = null!;

        [Required]
        public DateTime IssueDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "In Progress"; // In Progress, Completed
    }
}