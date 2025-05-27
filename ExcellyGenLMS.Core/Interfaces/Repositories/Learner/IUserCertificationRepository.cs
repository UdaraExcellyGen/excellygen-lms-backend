using System.Collections.Generic;
using System.Threading.Tasks;
using ExcellyGenLMS.Core.Entities.Learner;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.Learner
{
    public interface IUserCertificationRepository
    {
        Task<List<UserCertification>> GetUserCertificationsAsync(string userId);
        Task<UserCertification> GetUserCertificationByIdAsync(string userId, string certificationId);
    }
}