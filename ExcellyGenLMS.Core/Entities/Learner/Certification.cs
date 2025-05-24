using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExcellyGenLMS.Core.Entities.Learner
{
    public class Certification
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(200)]
        public required string Name { get; set; }

        [StringLength(200)]
        public string? IssuingOrganization { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(200)]
        public string? CredentialId { get; set; }

        [StringLength(255)]
        public string? ImagePath { get; set; } // Path to certification image

        public virtual ICollection<UserCertification> UserCertifications { get; set; } = new List<UserCertification>();
    }
}