using System.Collections.Generic;
using System.Threading.Tasks;
using ExcellyGenLMS.Core.Entities.Admin;
using ExcellyGenLMS.Core.Entities.Learner;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.Learner
{
    public interface IUserTechnologyRepository
    {
        Task<List<UserTechnology>> GetUserTechnologiesAsync(string userId);
        Task<List<Technology>> GetAvailableTechnologiesAsync(string userId);
        Task<UserTechnology> AddUserTechnologyAsync(string userId, string technologyId);
        Task RemoveUserTechnologyAsync(string userId, string technologyId);
    }
}