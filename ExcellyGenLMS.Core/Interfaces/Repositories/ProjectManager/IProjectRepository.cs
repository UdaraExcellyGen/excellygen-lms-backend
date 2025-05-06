using ExcellyGenLMS.Core.Entities.ProjectManager;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.ProjectManager
{
    public interface IProjectRepository
    {
        Task<IEnumerable<Project>> GetAllProjectsAsync();
        Task<Project?> GetProjectByIdAsync(string id);
        Task<Project> CreateProjectAsync(Project project);
        Task<Project> UpdateProjectAsync(Project project);
        Task DeleteProjectAsync(string id);
        Task<IEnumerable<Project>> GetProjectsByStatusAsync(string status);
    }
}