//ExcellyGenLMS.API\Controllers\ProjectManager\ProjectManagerController.cs


using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ExcellyGenLMS.Application.DTOs.ProjectManager;
using ExcellyGenLMS.Application.Interfaces.ProjectManager;
using ExcellyGenLMS.Application.Interfaces.Admin;
using AdminDto = ExcellyGenLMS.Application.DTOs.Admin;
using PMDto = ExcellyGenLMS.Application.DTOs.ProjectManager;

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
        private readonly ITechnologyService _technologyService;
        private readonly ILogger<ProjectManagerController>? _logger; // Mark as nullable

        public ProjectManagerController(
            IProjectService projectService,
            IRoleService roleService,
            IPMTechnologyService pmTechnologyService,
            ITechnologyService technologyService,
            ILogger<ProjectManagerController>? logger = null) // Mark parameter as nullable
        {
            _projectService = projectService;
            _roleService = roleService;
            _pmTechnologyService = pmTechnologyService;
            _technologyService = technologyService;
            _logger = logger; // This will no longer cause a warning
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
        public async Task<ActionResult<IEnumerable<PMDto.TechnologyDto>>> GetTechnologies()
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
        
        // New technology creation endpoint that properly sets creatorType
        [HttpPost("technologies")]
        public async Task<ActionResult<AdminDto.TechnologyDto>> CreateTechnology([FromBody] AdminDto.CreateTechnologyDto createTechnologyDto)
        {
            try
            {
                _logger?.LogInformation($"Creating technology: {createTechnologyDto.Name}");
                
                // Check for required data
                if (string.IsNullOrEmpty(createTechnologyDto.Name))
                {
                    return BadRequest(new { message = "Technology name is required" });
                }

                // Get user ID from claim for creator information
                var userId = User.FindFirst("uid")?.Value ?? "system";
                
                // Force creatorType to "project_manager" when using this endpoint
                var technology = await _technologyService.CreateTechnologyAsync(
                    createTechnologyDto, 
                    userId, 
                    "project_manager"  // <-- This ensures PM role is set as creator type
                );
                
                _logger?.LogInformation($"Technology created successfully with ID: {technology.Id}");
                
                return Ok(technology);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error creating technology: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while creating the technology", error = ex.Message });
            }
        }
        
        // Add technology update endpoint 
        [HttpPut("technologies/{id}")]
        public async Task<ActionResult<AdminDto.TechnologyDto>> UpdateTechnology(string id, [FromBody] AdminDto.UpdateTechnologyDto updateTechnologyDto)
        {
            try
            {
                _logger?.LogInformation($"Updating technology: {id}");
                
                // Flag indicating we're using the project manager role
                bool isAdmin = false;
                
                var technology = await _technologyService.UpdateTechnologyAsync(id, updateTechnologyDto, isAdmin);
                if (technology == null)
                {
                    return NotFound($"Technology with ID {id} not found");
                }

                return Ok(technology);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("cannot be modified"))
            {
                _logger?.LogWarning(ex, $"Permission error updating technology {id}");
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error updating technology {id}");
                return StatusCode(500, new { message = $"An error occurred while updating technology {id}", error = ex.Message });
            }
        }
        
        // Add technology deletion endpoint
        [HttpDelete("technologies/{id}")]
        public async Task<ActionResult> DeleteTechnology(string id)
        {
            try
            {
                _logger?.LogInformation($"Deleting technology: {id}");
                
                // Flag indicating we're using the project manager role
                bool isAdmin = false;
                
                await _technologyService.DeleteTechnologyAsync(id, isAdmin);
                return NoContent();
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("cannot be deleted") || ex.Message.Contains("in use"))
            {
                _logger?.LogWarning(ex, $"Permission error deleting technology {id}");
                return StatusCode(403, new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error deleting technology {id}");
                return StatusCode(500, new { message = $"An error occurred while deleting technology {id}", error = ex.Message });
            }
        }
    }
}