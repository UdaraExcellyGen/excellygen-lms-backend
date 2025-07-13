// ExcellyGenLMS.Core/Entities/Course/ExternalCertificate.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ExcellyGenLMS.Core.Entities.Auth;

namespace ExcellyGenLMS.Core.Entities.Course
{
    [Table("ExternalCertificates")]
    public class ExternalCertificate
    {
        [Key]
        [Column("external_certificate_id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        // Link to the User who owns the certificate
        [Required]
        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [Required]
        [MaxLength(500)]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        [Column("issuer")]
        public string Issuer { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("platform")]
        public string Platform { get; set; } = string.Empty;

        [Required]
        [Column("completion_date")]
        public DateTime CompletionDate { get; set; }

        [MaxLength(1000)]
        [Column("credential_url")]
        public string? CredentialUrl { get; set; }

        [MaxLength(200)]
        [Column("credential_id")]
        public string? CredentialId { get; set; }

        [MaxLength(2000)]
        [Column("description")]
        public string? Description { get; set; }

        [MaxLength(1000)]
        [Column("image_url")]
        public string? ImageUrl { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}