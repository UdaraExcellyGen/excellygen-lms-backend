using ExcellyGenLMS.Application.DTOs.ProjectManager;

namespace ExcellyGenLMS.Application.Interfaces.ProjectManager
{
    public interface IProjectService
    {
        Task<IEnumerable<ProjectDto>> GetAllProjectsAsync();
        Task<ProjectDto?> GetProjectByIdAsync(string id);
        Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto);
        Task<ProjectDto> UpdateProjectAsync(string id, UpdateProjectDto dto);
        Task DeleteProjectAsync(string id);
        Task<IEnumerable<ProjectDto>> GetProjectsByStatusAsync(string status);
    }
}