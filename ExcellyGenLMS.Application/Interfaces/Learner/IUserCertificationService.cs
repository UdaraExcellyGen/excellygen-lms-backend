using System.Collections.Generic;
using System.Threading.Tasks;
using ExcellyGenLMS.Application.DTOs.Learner;

namespace ExcellyGenLMS.Application.Interfaces.Learner
{
    public interface IUserCertificationService
    {
        Task<List<CertificationDto>> GetUserCertificationsAsync(string userId);
        Task<CertificationDto> GetUserCertificationByIdAsync(string userId, string certificationId);
    }
}