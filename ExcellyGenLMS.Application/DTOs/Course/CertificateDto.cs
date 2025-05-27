// ExcellyGenLMS.Application/DTOs/Course/CertificateDto.cs
using System;

namespace ExcellyGenLMS.Application.DTOs.Course
{
    public class CertificateDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty; // For convenience
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty; // For convenience
        public DateTime CompletionDate { get; set; }
        public string Title { get; set; } = string.Empty; // Title of the certificate
        public string CertificateFileUrl { get; set; } = string.Empty; // URL to the generated certificate file (e.g., PDF)
    }

    public class GenerateCertificateDto
    {
        public int CourseId { get; set; }
    }
}