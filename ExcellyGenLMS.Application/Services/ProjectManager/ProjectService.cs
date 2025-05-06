using ExcellyGenLMS.Application.DTOs.ProjectManager;
using ExcellyGenLMS.Application.Interfaces.ProjectManager;
using ExcellyGenLMS.Core.Entities.ProjectManager;
using ExcellyGenLMS.Core.Interfaces.Repositories.ProjectManager;

namespace ExcellyGenLMS.Application.Services.ProjectManager
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;

        public ProjectService(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<IEnumerable<ProjectDto>> GetAllProjectsAsync()
        {
            var projects = await _projectRepository.GetAllProjectsAsync();
            return projects.Select(ProjectToDto);
        }

        public async Task<ProjectDto?> GetProjectByIdAsync(string id)
        {
            var project = await _projectRepository.GetProjectByIdAsync(id);
            return project != null ? ProjectToDto(project) : null;
        }

        public async Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto)
        {
            var project = new Project
            {
                Name = dto.Name,
                Status = dto.Status ?? "Active",
                Deadline = dto.Deadline,
                StartDate = dto.StartDate,
                Description = dto.Description ?? string.Empty,
                ShortDescription = dto.ShortDescription ?? string.Empty,
                Progress = dto.Progress
            };

            // Add technologies
            if (dto.TechnologyIds != null)
            {
                foreach (var techId in dto.TechnologyIds)
                {
                    project.ProjectTechnologies.Add(new ProjectTechnology
                    {
                        TechnologyId = techId
                    });
                }
            }

            // Add roles
            if (dto.Roles != null)
            {
                foreach (var role in dto.Roles)
                {
                    project.ProjectRoles.Add(new ProjectRole
                    {
                        RoleName = role.RoleName,
                        RequiredCount = role.RequiredCount
                    });
                }
            }

            var createdProject = await _projectRepository.CreateProjectAsync(project);
            return ProjectToDto(createdProject);
        }

        public async Task<ProjectDto> UpdateProjectAsync(string id, UpdateProjectDto dto)
        {
            var project = await _projectRepository.GetProjectByIdAsync(id);
            if (project == null)
            {
                throw new KeyNotFoundException($"Project with id {id} not found");
            }

            project.Name = dto.Name;
            project.Status = dto.Status;
            project.Deadline = dto.Deadline;
            project.StartDate = dto.StartDate;
            project.Description = dto.Description ?? string.Empty;
            project.ShortDescription = dto.ShortDescription ?? string.Empty;
            project.Progress = dto.Progress;

            // Update technologies
            if (dto.TechnologyIds != null)
            {
                project.ProjectTechnologies.Clear();
                foreach (var techId in dto.TechnologyIds)
                {
                    project.ProjectTechnologies.Add(new ProjectTechnology
                    {
                        TechnologyId = techId
                    });
                }
            }

            // Update roles
            if (dto.Roles != null)
            {
                project.ProjectRoles.Clear();
                foreach (var role in dto.Roles)
                {
                    project.ProjectRoles.Add(new ProjectRole
                    {
                        RoleName = role.RoleName,
                        RequiredCount = role.RequiredCount
                    });
                }
            }

            var updatedProject = await _projectRepository.UpdateProjectAsync(project);
            return ProjectToDto(updatedProject);
        }

        public async Task DeleteProjectAsync(string id)
        {
            await _projectRepository.DeleteProjectAsync(id);
        }

        public async Task<IEnumerable<ProjectDto>> GetProjectsByStatusAsync(string status)
        {
            var projects = await _projectRepository.GetProjectsByStatusAsync(status);
            return projects.Select(ProjectToDto);
        }

        private ProjectDto ProjectToDto(Project project)
        {
            return new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Status = project.Status,
                Deadline = project.Deadline,
                StartDate = project.StartDate,
                Description = project.Description,
                ShortDescription = project.ShortDescription,
                Progress = project.Progress,
                TechnologyIds = project.ProjectTechnologies
                    .Select(pt => pt.TechnologyId)
                    .ToList(),
                Roles = project.ProjectRoles
                    .Select(pr => new ProjectRoleDto
                    {
                        RoleName = pr.RoleName,
                        RequiredCount = pr.RequiredCount
                    })
                    .ToList()
            };
        }
    }
}