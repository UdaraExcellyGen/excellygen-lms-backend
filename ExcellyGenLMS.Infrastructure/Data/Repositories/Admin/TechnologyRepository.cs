// Path: ExcellyGenLMS.Infrastructure.Data.Repositories.Admin.TechnologyRepository.cs

using ExcellyGenLMS.Core.Entities.Admin;
using ExcellyGenLMS.Core.Interfaces.Repositories.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Infrastructure.Data.Repositories.Admin
{
    public class TechnologyRepository : ITechnologyRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TechnologyRepository>? _logger;

        public TechnologyRepository(
            ApplicationDbContext context,
            ILogger<TechnologyRepository>? logger = null)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Technology>> GetAllTechnologiesAsync()
        {
            return await _context.Technologies.ToListAsync();
        }

        public async Task<Technology?> GetTechnologyByIdAsync(string id)
        {
            return await _context.Technologies.FindAsync(id);
        }

        public async Task<Technology> CreateTechnologyAsync(Technology technology)
        {
            // CRITICAL FIX: Log the exact entity we're saving to database
            _logger?.LogInformation($"REPOSITORY: Saving technology - Id={technology.Id}, Name={technology.Name}, CreatorType={technology.CreatorType}");
            
            // Double check that creator type is not null or empty
            if (string.IsNullOrEmpty(technology.CreatorType))
            {
                _logger?.LogWarning($"CreatorType is null or empty, defaulting to 'admin'");
                technology.CreatorType = "admin";
            }
            
            // Add the entity to context
            _context.Technologies.Add(technology);
            
            // Save changes and check for database-level issues
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Database error saving technology {technology.Id}");
                throw;
            }
            
            // Verify the entity was saved with correct creatorType
            var savedEntity = await _context.Technologies.FindAsync(technology.Id);
            if (savedEntity != null)
            {
                _logger?.LogInformation($"REPOSITORY: Saved technology - Id={savedEntity.Id}, Name={savedEntity.Name}, CreatorType={savedEntity.CreatorType}");
                
                // If creator type was lost or changed, fix it immediately
                if (savedEntity.CreatorType != technology.CreatorType)
                {
                    _logger?.LogWarning($"CreatorType mismatch detected! Original={technology.CreatorType}, Saved={savedEntity.CreatorType} - Fixing...");
                    
                    savedEntity.CreatorType = technology.CreatorType;
                    await _context.SaveChangesAsync();
                    
                    _logger?.LogInformation($"REPOSITORY: Fixed CreatorType - Now={savedEntity.CreatorType}");
                }
            }
            
            return technology;
        }

        public async Task<Technology> UpdateTechnologyAsync(Technology technology)
        {
            _logger?.LogInformation($"REPOSITORY: Updating technology - Id={technology.Id}, Name={technology.Name}");
            
            var existingTechnology = await _context.Technologies.FindAsync(technology.Id)
                ?? throw new KeyNotFoundException($"Technology with ID {technology.Id} not found");

            existingTechnology.Name = technology.Name;
            existingTechnology.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingTechnology;
        }

        public async Task DeleteTechnologyAsync(string id)
        {
            _logger?.LogInformation($"REPOSITORY: Deleting technology - Id={id}");
            
            var technology = await _context.Technologies.FindAsync(id)
                ?? throw new KeyNotFoundException($"Technology with ID {id} not found");
            
            _context.Technologies.Remove(technology);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsTechnologyInUseAsync(string id)
        {
            // Check if the technology is used in any projects
            return await _context.PMProjectTechnologies.AnyAsync(pt => pt.TechnologyId == id);
        }

        public async Task<Technology> ToggleTechnologyStatusAsync(string id)
        {
            _logger?.LogInformation($"REPOSITORY: Toggling status for technology - Id={id}");
            
            var technology = await GetTechnologyByIdAsync(id);
            if (technology == null)
                throw new KeyNotFoundException($"Technology with ID {id} not found");

            // Toggle status between active and inactive
            technology.Status = technology.Status == "active" ? "inactive" : "active";
            technology.UpdatedAt = DateTime.UtcNow;

            // Update and return
            await _context.SaveChangesAsync();
            return technology;
        }
    }
}