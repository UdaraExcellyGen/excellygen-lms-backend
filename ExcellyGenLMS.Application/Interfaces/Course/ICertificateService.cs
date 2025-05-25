// ExcellyGenLMS.Application/Interfaces/Course/ICertificateService.cs
using ExcellyGenLMS.Application.DTOs.Course;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Interfaces.Course
{
    public interface ICertificateService
    {
        Task<CertificateDto?> GetCertificateByIdAsync(int certificateId);
        Task<IEnumerable<CertificateDto>> GetCertificatesByUserIdAsync(string userId);
        Task<CertificateDto> GenerateCertificateAsync(string userId, int courseId);
    }
}