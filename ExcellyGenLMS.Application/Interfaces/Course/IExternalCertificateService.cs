// ExcellyGenLMS.Application/Interfaces/Course/IExternalCertificateService.cs
using ExcellyGenLMS.Application.DTOs.Course;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Interfaces.Course
{
    public interface IExternalCertificateService
    {
        Task<ExternalCertificateDto?> GetByIdAsync(string id);
        Task<IEnumerable<ExternalCertificateDto>> GetByUserIdAsync(string userId);
        Task<ExternalCertificateDto> AddAsync(string userId, AddExternalCertificateDto addDto);
        Task<ExternalCertificateDto> UpdateAsync(string userId, string certificateId, UpdateExternalCertificateDto updateDto);
        Task<bool> DeleteAsync(string userId, string certificateId);
        Task<bool> UserOwnsExternalCertificateAsync(string userId, string certificateId);
    }
}