using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExcellyGenLMS.Application.DTOs.Admin;
using ExcellyGenLMS.Application.DTOs.Learner;
using ExcellyGenLMS.Application.Interfaces.Learner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ExcellyGenLMS.API.Controllers.Learner
{
    [Route("api/user-technologies")]
    [ApiController]
    [Authorize]
    public class UserTechnologyController : ControllerBase
    {
        private readonly IUserTechnologyService _userTechnologyService;
        private readonly ILogger<UserTechnologyController> _logger;

        public UserTechnologyController(
            IUserTechnologyService userTechnologyService,
            ILogger<UserTechnologyController> logger)
        {
            _userTechnologyService = userTechnologyService;
            _logger = logger;
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<List<UserTechnologyDto>>> GetUserTechnologies(string userId)
        {
            try
            {
                _logger.LogInformation("Fetching technologies for user {UserId}", userId);
                var technologies = await _userTechnologyService.GetUserTechnologiesAsync(userId);
                return Ok(technologies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user technologies: {UserId}", userId);
                return StatusCode(500, new { error = "Failed to retrieve user technologies." });
            }
        }

        [HttpGet("{userId}/available")]
        public async Task<ActionResult<List<TechnologyDto>>> GetAvailableTechnologies(string userId)
        {
            try
            {
                _logger.LogInformation("Fetching available technologies for user {UserId}", userId);
                var technologies = await _userTechnologyService.GetAvailableTechnologiesAsync(userId);
                return Ok(technologies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available technologies: {UserId}", userId);
                return StatusCode(500, new { error = "Failed to retrieve available technologies." });
            }
        }

        [HttpPost("{userId}/{technologyId}")]
        public async Task<ActionResult<UserTechnologyDto>> AddUserTechnology(string userId, string technologyId)
        {
            try
            {
                _logger.LogInformation("Adding technology {TechnologyId} to user {UserId}", technologyId, userId);
                var userTechnology = await _userTechnologyService.AddUserTechnologyAsync(userId, technologyId);
                return Ok(userTechnology);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Technology or user not found: {UserId}, {TechnologyId}", userId, technologyId);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding technology: {UserId}, {TechnologyId}", userId, technologyId);
                return StatusCode(500, new { error = "Failed to add technology." });
            }
        }

        [HttpDelete("{userId}/{technologyId}")]
        public async Task<ActionResult> RemoveUserTechnology(string userId, string technologyId)
        {
            try
            {
                _logger.LogInformation("Removing technology {TechnologyId} from user {UserId}", technologyId, userId);
                await _userTechnologyService.RemoveUserTechnologyAsync(userId, technologyId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Technology or user not found: {UserId}, {TechnologyId}", userId, technologyId);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing technology: {UserId}, {TechnologyId}", userId, technologyId);
                return StatusCode(500, new { error = "Failed to remove technology." });
            }
        }
    }
}