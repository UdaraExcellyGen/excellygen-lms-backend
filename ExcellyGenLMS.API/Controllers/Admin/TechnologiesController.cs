// Path: ExcellyGenLMS.API/Controllers/Admin/TechnologiesController.cs

using ExcellyGenLMS.Application.DTOs.Admin;
using ExcellyGenLMS.Application.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ExcellyGenLMS.API.Controllers.Admin
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,ProjectManager")]  // Updated to include ProjectManager role
    public class TechnologiesController : ControllerBase
    {
        private readonly ITechnologyService _technologyService;
        private readonly ILogger<TechnologiesController> _logger;

        public TechnologiesController(
            ITechnologyService technologyService,
            ILogger<TechnologiesController> logger)
        {
            _technologyService = technologyService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<TechnologyDto>>> GetAllTechnologies()
        {
            try
            {
                var technologies = await _technologyService.GetAllTechnologiesAsync();
                return Ok(technologies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all technologies");
                return StatusCode(500, new { message = "An unexpected error occurred", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TechnologyDto>> GetTechnologyById(string id)
        {
            try
            {
                var technology = await _technologyService.GetTechnologyByIdAsync(id);
                return Ok(technology);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting technology with ID {id}");
                return StatusCode(500, new { message = "An unexpected error occurred", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<TechnologyDto>> CreateTechnology([FromBody] CreateTechnologyDto createTechnologyDto)
        {
            try
            {
                // Get current user ID
                string creatorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
                
                // Determine creator type based on active role
                string activeRole = Request.Headers["X-Active-Role"].FirstOrDefault() ?? "";
                string creatorType = activeRole.ToLower() == "admin" ? "admin" : "project_manager";
                
                _logger.LogInformation($"Creating technology with active role: {activeRole}, creator type: {creatorType}");

                var technology = await _technologyService.CreateTechnologyAsync(createTechnologyDto, creatorId, creatorType);
                return CreatedAtAction(nameof(GetTechnologyById), new { id = technology.Id }, technology);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating technology");
                return StatusCode(500, new { message = "An unexpected error occurred", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TechnologyDto>> UpdateTechnology(string id, [FromBody] UpdateTechnologyDto updateTechnologyDto)
        {
            try
            {
                // Check if the user is an admin based on active role
                string activeRole = Request.Headers["X-Active-Role"].FirstOrDefault() ?? "";
                bool isAdmin = activeRole.ToLower() == "admin";
                
                // For project managers, validate if they can update this technology
                if (!isAdmin)
                {
                    var tech = await _technologyService.GetTechnologyByIdAsync(id);
                    if (tech.CreatorType == "admin")
                    {
                        return Forbid("Admin-created technologies cannot be modified by project managers");
                    }
                    if (tech.Status != "active")
                    {
                        return BadRequest("Inactive technologies cannot be modified");
                    }
                }

                var technology = await _technologyService.UpdateTechnologyAsync(id, updateTechnologyDto, isAdmin);
                return Ok(technology);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating technology with ID {id}");
                return StatusCode(500, new { message = "An unexpected error occurred", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTechnology(string id)
        {
            try
            {
                // Check if the user is an admin based on active role
                string activeRole = Request.Headers["X-Active-Role"].FirstOrDefault() ?? "";
                bool isAdmin = activeRole.ToLower() == "admin";
                
                // For project managers, validate if they can delete this technology
                if (!isAdmin)
                {
                    var tech = await _technologyService.GetTechnologyByIdAsync(id);
                    if (tech.CreatorType == "admin")
                    {
                        return Forbid("Admin-created technologies cannot be deleted by project managers");
                    }
                    if (tech.Status != "active")
                    {
                        return BadRequest("Inactive technologies cannot be deleted");
                    }
                }

                await _technologyService.DeleteTechnologyAsync(id, isAdmin);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"Database error deleting technology with ID {id}");
                return BadRequest("This technology cannot be deleted because it is in use by one or more projects.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting technology with ID {id}");
                return StatusCode(500, new { message = "An unexpected error occurred", error = ex.Message });
            }
        }

        [HttpPatch("{id}/toggle-status")]
        [Authorize(Roles = "Admin")] // Only Admin can toggle status
        public async Task<ActionResult<TechnologyDto>> ToggleTechnologyStatus(string id)
        {
            try
            {
                var technology = await _technologyService.ToggleTechnologyStatusAsync(id);
                return Ok(technology);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error toggling status for technology with ID {id}");
                return StatusCode(500, new { message = "An unexpected error occurred", error = ex.Message });
            }
        }
    }
}