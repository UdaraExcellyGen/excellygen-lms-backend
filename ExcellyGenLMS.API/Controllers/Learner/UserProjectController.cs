using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExcellyGenLMS.Application.DTOs.Learner;
using ExcellyGenLMS.Application.Interfaces.Learner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ExcellyGenLMS.API.Controllers.Learner
{
    [Route("api/user-projects")]
    [ApiController]
    [Authorize]
    public class UserProjectController : ControllerBase
    {
        private readonly IUserProjectService _userProjectService;
        private readonly ILogger<UserProjectController> _logger;

        public UserProjectController(
            IUserProjectService userProjectService,
            ILogger<UserProjectController> logger)
        {
            _userProjectService = userProjectService;
            _logger = logger;
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<List<ProjectDto>>> GetUserProjects(string userId)
        {
            try
            {
                _logger.LogInformation("Fetching projects for user {UserId}", userId);
                var projects = await _userProjectService.GetUserProjectsAsync(userId);
                return Ok(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user projects: {UserId}", userId);
                return StatusCode(500, new { error = "Failed to retrieve user projects." });
            }
        }

        [HttpGet("{userId}/{projectId}")]
        public async Task<ActionResult<ProjectDto>> GetUserProject(string userId, string projectId)
        {
            try
            {
                _logger.LogInformation("Fetching project {ProjectId} for user {UserId}", projectId, userId);
                var project = await _userProjectService.GetUserProjectByIdAsync(userId, projectId);
                return Ok(project);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Project not found: {UserId}, {ProjectId}", userId, projectId);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user project: {UserId}, {ProjectId}", userId, projectId);
                return StatusCode(500, new { error = "Failed to retrieve user project." });
            }
        }
    }
}