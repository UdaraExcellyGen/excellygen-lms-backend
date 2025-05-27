using System;

namespace ExcellyGenLMS.Application.DTOs.Learner
{
    public class CertificationDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string IssuingOrganization { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IssueDate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CredentialId { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }
}