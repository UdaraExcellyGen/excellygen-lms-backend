// Path: ExcellyGenLMS.Application/Interfaces/ProjectManager/IProjectService.cs

using System.Collections.Generic;
using System.Threading.Tasks;
using ExcellyGenLMS.Application.DTOs.ProjectManager;

namespace ExcellyGenLMS.Application.Interfaces.ProjectManager
{
    public interface IProjectService
    {
        Task<IEnumerable<ProjectDto>> GetProjectsAsync(string? status = null);
        Task<ProjectDto?> GetProjectByIdAsync(string id);
        Task<ProjectDto> CreateProjectAsync(CreateProjectDto createProjectDto);
        Task<ProjectDto?> UpdateProjectAsync(string id, UpdateProjectDto updateProjectDto);
        Task<bool> DeleteProjectAsync(string id);
    }
}