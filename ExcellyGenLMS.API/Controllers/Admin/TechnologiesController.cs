// Path: ExcellyGenLMS.API.Controllers.Admin.TechnologiesController.cs
using ExcellyGenLMS.Application.DTOs.Admin;
using ExcellyGenLMS.Application.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExcellyGenLMS.API.Controllers.Admin
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,ProjectManager")]
    public class TechnologiesController : ControllerBase
    {
        private readonly ITechnologyService _technologyService;
        private readonly ILogger<TechnologiesController>? _logger;

        public TechnologiesController(
            ITechnologyService technologyService,
            ILogger<TechnologiesController>? logger = null)
        {
            _technologyService = technologyService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<TechnologyDto>>> GetAllTechnologies()
        {
            var technologies = await _technologyService.GetAllTechnologiesAsync();
            return Ok(technologies);
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
        }

        [HttpPost]
        public async Task<ActionResult<TechnologyDto>> CreateTechnology([FromBody] CreateTechnologyDto createTechnologyDto)
        {
            try
            {
                // Log all request headers for debugging
                _logger?.LogInformation("REQUEST HEADERS:");
                foreach (var header in Request.Headers)
                {
                    _logger?.LogInformation($"  {header.Key}: {header.Value}");
                }
                
                // Log the request body
                _logger?.LogInformation($"Creating technology: {createTechnologyDto.Name}");
                
                // Get the current user ID from claims
                var userId = User.FindFirst("uid")?.Value ?? "system";
                
                // CRITICAL FIX: Determine creator type from role
                string creatorType;
                
                // First check explicit header
                if (Request.Headers.TryGetValue("X-Creator-Type", out var creatorTypeHeader))
                {
                    creatorType = creatorTypeHeader.ToString();
                    _logger?.LogInformation($"Using explicit creator type header: {creatorType}");
                }
                // Then check role header
                else if (Request.Headers.TryGetValue("X-Active-Role", out var roleHeader))
                {
                    creatorType = roleHeader.ToString().Equals("ProjectManager", StringComparison.OrdinalIgnoreCase)
                        ? "project_manager"
                        : "admin";
                    _logger?.LogInformation($"Derived creator type from role header: {roleHeader} → {creatorType}");
                }
                // Otherwise check actual user role claims
                else
                {
                    var isProjectManager = User.IsInRole("ProjectManager");
                    var isAdmin = User.IsInRole("Admin");
                    
                    // If the user has ProjectManager role, set that as the creator type
                    creatorType = isProjectManager ? "project_manager" : "admin";
                    _logger?.LogInformation($"Derived creator type from role claims: PM={isProjectManager}, Admin={isAdmin} → {creatorType}");
                }
                
                // If name has PM prefix, ensure correct type
                if (createTechnologyDto.Name.StartsWith("[PM]"))
                {
                    creatorType = "project_manager";
                    _logger?.LogInformation("Found [PM] prefix in name, forcing creator type to project_manager");
                }
                
                // Create the technology with explicit creator type
                var technology = await _technologyService.CreateTechnologyAsync(createTechnologyDto, userId, creatorType);
                
                // Log what was returned
                _logger?.LogInformation($"Technology created: Id={technology.Id}, Name={technology.Name}, CreatorType={technology.CreatorType}");
                
                return CreatedAtAction(nameof(GetTechnologyById), new { id = technology.Id }, technology);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error creating technology: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while creating the technology", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TechnologyDto>> UpdateTechnology(string id, [FromBody] UpdateTechnologyDto updateTechnologyDto)
        {
            try
            {
                // Determine if user is acting as admin
                bool isAdmin = false;
                if (Request.Headers.TryGetValue("X-Active-Role", out var roleHeader))
                {
                    isAdmin = roleHeader.ToString().Equals("Admin", StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    isAdmin = User.IsInRole("Admin");
                }
                
                _logger?.LogInformation($"Updating technology {id} as {(isAdmin ? "Admin" : "Project Manager")}");
                
                var technology = await _technologyService.UpdateTechnologyAsync(id, updateTechnologyDto, isAdmin);
                return Ok(technology);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
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

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTechnology(string id)
        {
            try
            {
                // Determine if user is acting as admin
                bool isAdmin = false;
                if (Request.Headers.TryGetValue("X-Active-Role", out var roleHeader))
                {
                    isAdmin = roleHeader.ToString().Equals("Admin", StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    isAdmin = User.IsInRole("Admin");
                }
                
                _logger?.LogInformation($"Deleting technology {id} as {(isAdmin ? "Admin" : "Project Manager")}");
                
                await _technologyService.DeleteTechnologyAsync(id, isAdmin);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger?.LogWarning(ex, $"Permission error deleting technology {id}");
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error deleting technology {id}");
                return StatusCode(500, new { message = $"An error occurred while deleting technology {id}", error = ex.Message });
            }
        }

        [HttpPatch("{id}/toggle-status")]
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
        }
    }
}