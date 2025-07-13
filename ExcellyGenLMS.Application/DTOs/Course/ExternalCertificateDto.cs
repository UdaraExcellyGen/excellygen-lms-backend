// ExcellyGenLMS.Application/DTOs/Course/ExternalCertificateDto.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace ExcellyGenLMS.Application.DTOs.Course
{
    public class ExternalCertificateDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty; // For convenience
        public string Title { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public DateTime CompletionDate { get; set; }
        public string? CredentialUrl { get; set; }
        public string? CredentialId { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class AddExternalCertificateDto
    {
        [Required]
        [MaxLength(500)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Issuer { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Platform { get; set; } = string.Empty;

        [Required]
        public DateTime CompletionDate { get; set; }

        [MaxLength(1000)]
        [Url]
        public string? CredentialUrl { get; set; }

        [MaxLength(200)]
        public string? CredentialId { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }
    }

    public class UpdateExternalCertificateDto
    {
        [MaxLength(500)]
        public string? Title { get; set; }

        [MaxLength(200)]
        public string? Issuer { get; set; }

        [MaxLength(100)]
        public string? Platform { get; set; }

        public DateTime? CompletionDate { get; set; }

        [MaxLength(1000)]
        [Url]
        public string? CredentialUrl { get; set; }

        [MaxLength(200)]
        public string? CredentialId { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }
    }
}