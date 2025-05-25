using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ExcellyGenLMS.Core.Entities.Auth; // For User entity

namespace ExcellyGenLMS.Core.Entities.Course
{
    [Table("Certificates")]
    public class Certificate
    {
        [Key]
        [Column("certificate_id")]
        public int Id { get; set; }

        // Link to the User who earned the certificate
        [Required]
        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!; // Non-nullable relationship

        [Column("completion_date")]
        public DateTime CompletionDate { get; set; }

        [Column("certificate_data")]
        [Required]
        public required byte[] CertificateData { get; set; } // The actual binary data of the certificate document

        [Column("certificate_title")]
        [Required]
        public required string Title { get; set; } // The title of the certificate (e.g., "Web Development Fundamentals")

        // Link to the Course for which the certificate was issued
        [Required]
        [Column("course_id")]
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!; // Non-nullable relationship

        // ADDED: Path to the stored certificate file (e.g., PDF)
        [MaxLength(1024)] // Increased length for paths/URLs
        public string? FilePath { get; set; }
    }
}