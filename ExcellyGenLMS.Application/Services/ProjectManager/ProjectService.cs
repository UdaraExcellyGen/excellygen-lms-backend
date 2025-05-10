using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ExcellyGenLMS.Application.DTOs.ProjectManager;
using ExcellyGenLMS.Application.Interfaces.ProjectManager;
using ExcellyGenLMS.Core.Entities.ProjectManager;
using ExcellyGenLMS.Core.Interfaces.Repositories.ProjectManager;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ExcellyGenLMS.Application.Services.ProjectManager
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ProjectService>? _logger; // Mark logger as nullable

        public ProjectService(
            IProjectRepository projectRepository,
            IRoleRepository roleRepository,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ProjectService>? logger = null) // Accept nullable logger
        {
            _projectRepository = projectRepository;
            _roleRepository = roleRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger; // Can be null
        }

        public async Task<IEnumerable<ProjectDto>> GetProjectsAsync(string? status = null)
        {
            var projects = await _projectRepository.GetAllAsync(status);
            return projects.Select(MapProjectToDto);
        }

        public async Task<ProjectDto?> GetProjectByIdAsync(string id)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null)
            {
                return null;
            }
            
            return MapProjectToDto(project);
        }

        public async Task<ProjectDto> CreateProjectAsync(CreateProjectDto createProjectDto)
        {
            // Try to get user ID from current authenticated user
            string userId = GetCurrentUserId();
            _logger?.LogInformation($"Creating project with user ID: {userId}");

            var project = new PMProject
            {
                Name = createProjectDto.Name,
                Description = createProjectDto.Description ?? string.Empty,
                ShortDescription = createProjectDto.ShortDescription ?? string.Empty,
                Status = createProjectDto.Status ?? "Active",
                // StartDate is non-nullable DateTime
                StartDate = createProjectDto.StartDate,  // Use directly
                Deadline = createProjectDto.Deadline,    // Deadline is already nullable
                Progress = createProjectDto.Progress,
                CreatorId = userId,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                var createdProject = await _projectRepository.AddAsync(project);
                _logger?.LogInformation($"Project created with ID: {createdProject.Id}");

                // Add technologies
                if (createProjectDto.RequiredTechnologyIds?.Any() == true)
                {
                    await _projectRepository.UpdateTechnologiesAsync(createdProject.Id, createProjectDto.RequiredTechnologyIds);
                    _logger?.LogInformation($"Added {createProjectDto.RequiredTechnologyIds.Count} technologies to project");
                }

                // Add required roles
                if (createProjectDto.RequiredRoles?.Any() == true)
                {
                    var requiredRoles = createProjectDto.RequiredRoles
                        .Select(r => new PMProjectRequiredRole
                        {
                            ProjectId = createdProject.Id,
                            Role = r.RoleId,
                            Count = r.Count
                        })
                        .ToList();

                    await _projectRepository.UpdateRequiredRolesAsync(createdProject.Id, requiredRoles);
                    _logger?.LogInformation($"Added {requiredRoles.Count} roles to project");
                }

                // Reload the project with all relationships
                var updatedProject = await _projectRepository.GetByIdAsync(createdProject.Id);
                if (updatedProject == null)
                {
                    throw new InvalidOperationException("Created project could not be retrieved");
                }
                
                return MapProjectToDto(updatedProject);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error creating project: {ex.Message}");
                throw;
            }
        }

        public async Task<ProjectDto?> UpdateProjectAsync(string id, UpdateProjectDto updateProjectDto)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null)
            {
                return null;
            }

            project.Name = updateProjectDto.Name;
            project.Description = updateProjectDto.Description ?? string.Empty;
            project.ShortDescription = updateProjectDto.ShortDescription ?? string.Empty;
            project.Status = updateProjectDto.Status;
            // StartDate is non-nullable DateTime - use directly
            project.StartDate = updateProjectDto.StartDate;  
            project.Deadline = updateProjectDto.Deadline;    // Deadline is already nullable
            project.Progress = updateProjectDto.Progress;
            project.UpdatedAt = DateTime.UtcNow;

            var updatedProject = await _projectRepository.UpdateAsync(project);

            // Update technologies
            await _projectRepository.UpdateTechnologiesAsync(updatedProject.Id, updateProjectDto.RequiredTechnologyIds);

            // Update required roles
            var requiredRoles = updateProjectDto.RequiredRoles
                .Select(r => new PMProjectRequiredRole
                {
                    ProjectId = updatedProject.Id,
                    Role = r.RoleId,
                    Count = r.Count
                })
                .ToList();

            await _projectRepository.UpdateRequiredRolesAsync(updatedProject.Id, requiredRoles);

            // Reload the project with all relationships
            var refreshedProject = await _projectRepository.GetByIdAsync(updatedProject.Id);
            if (refreshedProject == null)
            {
                throw new InvalidOperationException("Updated project could not be retrieved");
            }
            
            return MapProjectToDto(refreshedProject);
        }

        public async Task<bool> DeleteProjectAsync(string id)
        {
            return await _projectRepository.DeleteAsync(id);
        }

        private ProjectDto MapProjectToDto(PMProject project)
        {
            var requiredRoles = project.RequiredRoles
                .Select(r => new RequiredRoleDto
                {
                    RoleId = r.Role,
                    RoleName = r.Role, // This should ideally fetch the actual role name
                    Count = r.Count
                })
                .ToList();

            return new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description ?? string.Empty,
                ShortDescription = project.ShortDescription ?? string.Empty,
                Status = project.Status,
                StartDate = project.StartDate,
                Deadline = project.Deadline,
                Progress = project.Progress,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                CreatorId = project.CreatorId,
                CreatorName = project.Creator?.Name ?? "Unknown",
                
                RequiredSkills = project.Technologies
                    .Select(t => new TechnologyDto
                    {
                        Id = t.TechnologyId,
                        Name = t.Technology?.Name ?? "Unknown"
                    })
                    .ToList(),
                
                RequiredRoles = requiredRoles
            };
        }

        private string GetCurrentUserId()
        {
            try
            {
                // Try different claim types to find user ID
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                {
                    _logger?.LogWarning("HTTP context is null - using default user");
                    return "system"; // Fallback value
                }

                var user = httpContext.User;
                if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
                {
                    _logger?.LogWarning("User not authenticated - using default user");
                    return "system"; // Fallback value
                }

                // Attempt to get user ID from various claims
                var userId = user.FindFirst("uid")?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    return userId;
                }

                userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    return userId;
                }

                userId = user.FindFirst("sub")?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    return userId;
                }

                // Log all claims for debugging
                var claims = user.Claims.ToList();
                if (claims.Any())
                {
                    _logger?.LogInformation("Available claims:");
                    foreach (var claim in claims)
                    {
                        _logger?.LogInformation($"  {claim.Type}: {claim.Value}");
                    }
                }
                else
                {
                    _logger?.LogWarning("No claims found in user principal");
                }

                userId = user.Identity?.Name;
                if (!string.IsNullOrEmpty(userId))
                {
                    return userId;
                }

                // If still not found, use a default ID
                _logger?.LogWarning("Could not find user ID in claims - using default user");
                return "system"; // Fallback value
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting current user ID");
                return "system"; // Fallback on exception
            }
        }
    }
}