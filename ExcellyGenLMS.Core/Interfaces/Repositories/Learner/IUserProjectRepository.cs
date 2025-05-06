using System.Collections.Generic;
using System.Threading.Tasks;
using ExcellyGenLMS.Core.Entities.Learner;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.Learner
{
    public interface IUserProjectRepository
    {
        Task<List<Project>> GetUserProjectsAsync(string userId);
        Task<Project> GetUserProjectByIdAsync(string userId, string projectId);
    }
}