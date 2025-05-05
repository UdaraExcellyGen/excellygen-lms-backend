using System.Collections.Generic;
using System.Threading.Tasks;
using ExcellyGenLMS.Application.DTOs.Admin;
using ExcellyGenLMS.Application.DTOs.Learner;

namespace ExcellyGenLMS.Application.Interfaces.Learner
{
    public interface IUserTechnologyService
    {
        Task<List<UserTechnologyDto>> GetUserTechnologiesAsync(string userId);
        Task<List<TechnologyDto>> GetAvailableTechnologiesAsync(string userId);
        Task<UserTechnologyDto> AddUserTechnologyAsync(string userId, string technologyId);
        Task RemoveUserTechnologyAsync(string userId, string technologyId);
    }
}