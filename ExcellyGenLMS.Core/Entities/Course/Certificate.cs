// Certificate.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExcellyGenLMS.Core.Entities.Course
{
    [Table("Certificates")]
    public class Certificate
    {
        [Key]
        [Column("certificate_id")]
        public int Id { get; set; }

        [Column("completion_date")]
        public DateTime CompletionDate { get; set; }

        [Column("certificate_data")]
        public required byte[] CertificateData { get; set; } // Added required modifier

        [Column("certificate_title")]
        [Required]
        public required string Title { get; set; } // Added required modifier
    }
}