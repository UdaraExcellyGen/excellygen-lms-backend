using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ExcellyGenLMS.Application.DTOs.ProjectManager;
using ExcellyGenLMS.Application.Interfaces.ProjectManager;

namespace ExcellyGenLMS.API.Controllers.ProjectManager
{
    [ApiController]
    [Authorize(Roles = "Admin,ProjectManager")]
    [Route("api/project-manager")]
    public class ProjectManagerController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly IRoleService _roleService;
        private readonly IPMTechnologyService _pmTechnologyService;
        private readonly ILogger<ProjectManagerController> _logger;

        public ProjectManagerController(
            IProjectService projectService,
            IRoleService roleService,
            IPMTechnologyService pmTechnologyService,
            ILogger<ProjectManagerController> logger = null)
        {
            _projectService = projectService;
            _roleService = roleService;
            _pmTechnologyService = pmTechnologyService;
            _logger = logger;
        }

        // ----- Project Endpoints -----

        [HttpGet("projects")]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetProjects([FromQuery] string? status = null)
        {
            try
            {
                var projects = await _projectService.GetProjectsAsync(status);
                return Ok(projects);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error fetching projects");
                return StatusCode(500, new { message = "An error occurred while fetching projects", error = ex.Message });
            }
        }

        [HttpGet("projects/{id}")]
        public async Task<ActionResult<ProjectDto>> GetProject(string id)
        {
            try
            {
                var project = await _projectService.GetProjectByIdAsync(id);
                if (project == null)
                {
                    return NotFound($"Project with ID {id} not found");
                }

                return Ok(project);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error fetching project {id}");
                return StatusCode(500, new { message = $"An error occurred while fetching project {id}", error = ex.Message });
            }
        }

        [HttpPost("projects")]
        public async Task<ActionResult<ProjectDto>> CreateProject([FromBody] CreateProjectDto createProjectDto)
        {
            try
            {
                // Log the received data
                _logger?.LogInformation($"Creating project: {createProjectDto.Name}");
                
                // Check for required data
                if (string.IsNullOrEmpty(createProjectDto.Name))
                {
                    return BadRequest(new { message = "Project name is required" });
                }

                var project = await _projectService.CreateProjectAsync(createProjectDto);
                _logger?.LogInformation($"Project created successfully with ID: {project.Id}");
                
                return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error creating project: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while creating the project", error = ex.Message });
            }
        }

        [HttpPut("projects/{id}")]
        public async Task<ActionResult<ProjectDto>> UpdateProject(string id, [FromBody] UpdateProjectDto updateProjectDto)
        {
            try
            {
                var project = await _projectService.UpdateProjectAsync(id, updateProjectDto);
                if (project == null)
                {
                    return NotFound($"Project with ID {id} not found");
                }

                return Ok(project);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error updating project {id}");
                return StatusCode(500, new { message = $"An error occurred while updating project {id}", error = ex.Message });
            }
        }

        [HttpDelete("projects/{id}")]
        public async Task<ActionResult> DeleteProject(string id)
        {
            try
            {
                var result = await _projectService.DeleteProjectAsync(id);
                if (!result)
                {
                    return NotFound($"Project with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error deleting project {id}");
                return StatusCode(500, new { message = $"An error occurred while deleting project {id}", error = ex.Message });
            }
        }

        // ----- Role Endpoints -----

        [HttpGet("roles")]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetRoles()
        {
            try
            {
                var roles = await _roleService.GetRolesAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error fetching roles");
                return StatusCode(500, new { message = "An error occurred while fetching roles", error = ex.Message });
            }
        }

        [HttpGet("roles/{id}")]
        public async Task<ActionResult<RoleDto>> GetRole(string id)
        {
            try
            {
                var role = await _roleService.GetRoleByIdAsync(id);
                if (role == null)
                {
                    return NotFound($"Role with ID {id} not found");
                }

                return Ok(role);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error fetching role {id}");
                return StatusCode(500, new { message = $"An error occurred while fetching role {id}", error = ex.Message });
            }
        }

        [HttpPost("roles")]
        public async Task<ActionResult<RoleDto>> CreateRole([FromBody] CreateRoleDto createRoleDto)
        {
            try
            {
                var role = await _roleService.CreateRoleAsync(createRoleDto);
                return CreatedAtAction(nameof(GetRole), new { id = role.Id }, role);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating role");
                return StatusCode(500, new { message = "An error occurred while creating the role", error = ex.Message });
            }
        }

        [HttpPut("roles/{id}")]
        public async Task<ActionResult<RoleDto>> UpdateRole(string id, [FromBody] UpdateRoleDto updateRoleDto)
        {
            try
            {
                var role = await _roleService.UpdateRoleAsync(id, updateRoleDto);
                if (role == null)
                {
                    return NotFound($"Role with ID {id} not found");
                }

                return Ok(role);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error updating role {id}");
                return StatusCode(500, new { message = $"An error occurred while updating role {id}", error = ex.Message });
            }
        }

        [HttpDelete("roles/{id}")]
        public async Task<ActionResult> DeleteRole(string id)
        {
            try
            {
                var result = await _roleService.DeleteRoleAsync(id);
                if (!result)
                {
                    return NotFound($"Role with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error deleting role {id}");
                return StatusCode(500, new { message = $"An error occurred while deleting role {id}", error = ex.Message });
            }
        }

        // ----- Technology Endpoints -----

        [HttpGet("technologies")]
        public async Task<ActionResult<IEnumerable<TechnologyDto>>> GetTechnologies()
        {
            try
            {
                var technologies = await _pmTechnologyService.GetTechnologiesAsync();
                return Ok(technologies);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error fetching technologies");
                return StatusCode(500, new { message = "An error occurred while fetching technologies", error = ex.Message });
            }
        }
    }
}
