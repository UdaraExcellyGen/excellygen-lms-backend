// ExcellyGenLMS.Core/Interfaces/Repositories/Course/ICertificateRepository.cs
using ExcellyGenLMS.Core.Entities.Course;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.Course
{
    public interface ICertificateRepository
    {
        Task<Certificate?> GetByIdAsync(int id);
        Task<IEnumerable<Certificate>> GetCertificatesByUserIdAsync(string userId);
        Task<Certificate?> GetCertificateByUserIdAndCourseIdAsync(string userId, int courseId);
        Task<Certificate> AddAsync(Certificate certificate);
        Task UpdateAsync(Certificate certificate); // Not typically needed for certificates, but for completeness
        Task DeleteAsync(int id);
    }
}