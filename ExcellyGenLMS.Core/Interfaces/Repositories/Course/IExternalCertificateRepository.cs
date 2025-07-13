// ExcellyGenLMS.Core/Interfaces/Repositories/Course/IExternalCertificateRepository.cs
using ExcellyGenLMS.Core.Entities.Course;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.Course
{
    public interface IExternalCertificateRepository
    {
        Task<ExternalCertificate?> GetByIdAsync(string id);
        Task<IEnumerable<ExternalCertificate>> GetByUserIdAsync(string userId);
        Task<ExternalCertificate> AddAsync(ExternalCertificate externalCertificate);
        Task<ExternalCertificate> UpdateAsync(ExternalCertificate externalCertificate);
        Task<bool> DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
        Task<bool> UserOwnsExternalCertificateAsync(string userId, string certificateId);
    }
}