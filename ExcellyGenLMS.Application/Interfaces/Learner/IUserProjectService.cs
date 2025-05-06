using System.Collections.Generic;
using System.Threading.Tasks;
using ExcellyGenLMS.Application.DTOs.Learner;

namespace ExcellyGenLMS.Application.Interfaces.Learner
{
    public interface IUserProjectService
    {
        Task<List<ProjectDto>> GetUserProjectsAsync(string userId);
        Task<ProjectDto> GetUserProjectByIdAsync(string userId, string projectId);
    }
}