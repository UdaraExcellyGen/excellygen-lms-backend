// Path: ExcellyGenLMS.Application/Services/Admin/TechnologyService.cs

using ExcellyGenLMS.Application.DTOs.Admin;
using ExcellyGenLMS.Application.Interfaces.Admin;
using ExcellyGenLMS.Core.Entities.Admin;
using ExcellyGenLMS.Core.Interfaces.Repositories.Admin;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Services.Admin
{
    public class TechnologyService : ITechnologyService
    {
        private readonly ITechnologyRepository _technologyRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<TechnologyService>? _logger;

        public TechnologyService(
            ITechnologyRepository technologyRepository,
            IHttpContextAccessor httpContextAccessor,
            ILogger<TechnologyService>? logger = null)
        {
            _technologyRepository = technologyRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<List<TechnologyDto>> GetAllTechnologiesAsync()
        {
            var technologies = await _technologyRepository.GetAllTechnologiesAsync();
            return technologies.Select(MapToDto).ToList();
        }

        public async Task<TechnologyDto> GetTechnologyByIdAsync(string id)
        {
            var technology = await _technologyRepository.GetTechnologyByIdAsync(id);
            if (technology == null)
                throw new KeyNotFoundException($"Technology with ID {id} not found");

            return MapToDto(technology);
        }

        public async Task<TechnologyDto> CreateTechnologyAsync(CreateTechnologyDto createTechnologyDto, string creatorId = "system", string? creatorType = null)
        {
            // Log incoming parameters
            _logger?.LogInformation($"CreateTechnologyAsync called with: Name={createTechnologyDto.Name}, CreatorId={creatorId}, CreatorType={creatorType}");
            
            // CRITICAL FIX: Ensure creator type is correctly determined
            if (string.IsNullOrEmpty(creatorType))
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    // First check if there's an explicit creator type header
                    if (httpContext.Request.Headers.TryGetValue("X-Creator-Type", out var creatorTypeHeader))
                    {
                        creatorType = creatorTypeHeader.ToString();
                        _logger?.LogInformation($"Using creator type from header: {creatorType}");
                    }
                    // Then check for role header
                    else if (httpContext.Request.Headers.TryGetValue("X-Active-Role", out var roleHeader))
                    {
                        var roleName = roleHeader.ToString();
                        creatorType = roleName.Equals("ProjectManager", StringComparison.OrdinalIgnoreCase) 
                            ? "project_manager" 
                            : "admin";
                        _logger?.LogInformation($"Using creator type derived from role header: {roleName} → {creatorType}");
                    }
                    // Finally check role claims if no headers
                    else
                    {
                        bool isProjectManager = httpContext.User.IsInRole("ProjectManager");
                        bool isAdmin = httpContext.User.IsInRole("Admin");
                        
                        // If user has both roles, use the more specific one (ProjectManager)
                        // or determine based on context (current URL path, etc.)
                        if (isProjectManager)
                        {
                            creatorType = "project_manager";
                            _logger?.LogInformation("Setting creator type to project_manager based on role claim");
                        }
                        else if (isAdmin)
                        {
                            creatorType = "admin";
                            _logger?.LogInformation("Setting creator type to admin based on role claim");
                        }
                        else
                        {
                            creatorType = "admin"; // Default if no role detected
                            _logger?.LogWarning("No specific role detected, defaulting to admin creator type");
                        }
                    }
                }
                else
                {
                    // Default if no HTTP context
                    creatorType = "admin";
                    _logger?.LogWarning("No HTTP context available, defaulting to admin creator type");
                }
            }
            else
            {
                _logger?.LogInformation($"Using explicitly provided creator type: {creatorType}");
            }
            
            // Check for PM indicator in name and ensure creator type is consistent
            if (createTechnologyDto.Name.StartsWith("[PM]") && creatorType != "project_manager")
            {
                creatorType = "project_manager";
                _logger?.LogInformation("Detected [PM] prefix in name, forcing creator type to project_manager");
            }
            
            // Create the technology entity
            var technology = new Technology
            {
                Id = Guid.NewGuid().ToString(),
                Name = createTechnologyDto.Name,
                Status = "active",
                CreatedAt = DateTime.UtcNow,
                CreatorId = creatorId,
                CreatorType = creatorType // This is now properly determined
            };
            
            // CRITICAL: Log the exact entity we're passing to repository
            _logger?.LogInformation($"Sending technology to repository: Id={technology.Id}, Name={technology.Name}, CreatorType={technology.CreatorType}");
            
            var createdTechnology = await _technologyRepository.CreateTechnologyAsync(technology);
            
            // Verify the saved entity has the correct creator type
            _logger?.LogInformation($"Technology created: Id={createdTechnology.Id}, CreatorType={createdTechnology.CreatorType}");
            
            // If creator type was changed during save, log a warning
            if (createdTechnology.CreatorType != technology.CreatorType)
            {
                _logger?.LogWarning($"Creator type was changed during save from {technology.CreatorType} to {createdTechnology.CreatorType}");
            }
            
            return MapToDto(createdTechnology);
        }

        public async Task<TechnologyDto> UpdateTechnologyAsync(string id, UpdateTechnologyDto updateTechnologyDto, bool isAdmin = false)
        {
            var technology = await _technologyRepository.GetTechnologyByIdAsync(id);
            if (technology == null)
                throw new KeyNotFoundException($"Technology with ID {id} not found");

            // If not specified as admin, check from request context
            if (!isAdmin)
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    // First check role from header
                    if (httpContext.Request.Headers.TryGetValue("X-Active-Role", out var roleHeader))
                    {
                        isAdmin = roleHeader.ToString().Equals("Admin", StringComparison.OrdinalIgnoreCase);
                    }
                    else // Then check role claims
                    {
                        isAdmin = httpContext.User.IsInRole("Admin");
                    }
                }
            }

            // Check permissions
            if (!isAdmin && technology.CreatorType == "admin")
                throw new InvalidOperationException("Admin-created technologies cannot be modified by project managers");

            if (!isAdmin && technology.Status != "active")
                throw new InvalidOperationException("Inactive technologies cannot be modified");

            technology.Name = updateTechnologyDto.Name;
            technology.UpdatedAt = DateTime.UtcNow;

            var updatedTechnology = await _technologyRepository.UpdateTechnologyAsync(technology);
            return MapToDto(updatedTechnology);
        }

        public async Task DeleteTechnologyAsync(string id, bool isAdmin = false)
        {
            var technology = await _technologyRepository.GetTechnologyByIdAsync(id);
            if (technology == null)
                throw new KeyNotFoundException($"Technology with ID {id} not found");

            // If not specified as admin, check from request context
            if (!isAdmin)
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    // First check role from header
                    if (httpContext.Request.Headers.TryGetValue("X-Active-Role", out var roleHeader))
                    {
                        isAdmin = roleHeader.ToString().Equals("Admin", StringComparison.OrdinalIgnoreCase);
                    }
                    else // Then check role claims
                    {
                        isAdmin = httpContext.User.IsInRole("Admin");
                    }
                }
            }

            // Check permissions
            if (!isAdmin && technology.CreatorType == "admin")
                throw new InvalidOperationException("Admin-created technologies cannot be deleted by project managers");

            if (!isAdmin && technology.Status != "active")
                throw new InvalidOperationException("Inactive technologies cannot be deleted");

            // Check if in use
            bool isInUse = await _technologyRepository.IsTechnologyInUseAsync(id);
            if (isInUse)
                throw new InvalidOperationException("Cannot delete technology that is in use by projects");

            await _technologyRepository.DeleteTechnologyAsync(id);
        }

        public async Task<TechnologyDto> ToggleTechnologyStatusAsync(string id)
        {
            var technology = await _technologyRepository.ToggleTechnologyStatusAsync(id);
            return MapToDto(technology);
        }

        // Helper method to map Technology entity to TechnologyDto
        private static TechnologyDto MapToDto(Technology technology)
        {
            return new TechnologyDto
            {
                Id = technology.Id,
                Name = technology.Name,
                Status = technology.Status,
                CreatedAt = technology.CreatedAt,
                UpdatedAt = technology.UpdatedAt,
                CreatorId = technology.CreatorId ?? "system",
                CreatorType = technology.CreatorType ?? "admin"
            };
        }
    }
}