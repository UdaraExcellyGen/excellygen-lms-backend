//ExcellyGenLMS.Infrastructure\Data\Repositories\Admin\TechnologyRepository.cs
using ExcellyGenLMS.Core.Entities.Admin;
using ExcellyGenLMS.Core.Interfaces.Repositories.Admin;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Infrastructure.Data.Repositories.Admin
{
    public class TechnologyRepository : ITechnologyRepository
    {
        private readonly ApplicationDbContext _context;

        public TechnologyRepository(ApplicationDbContext context)
        {
            _context = context;
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
            _context.Technologies.Add(technology);
            await _context.SaveChangesAsync();
            return technology;
        }

        public async Task<Technology> UpdateTechnologyAsync(Technology technology)
        {
            var existingTechnology = await _context.Technologies.FindAsync(technology.Id)
                ?? throw new KeyNotFoundException($"Technology with ID {technology.Id} not found");

            existingTechnology.Name = technology.Name;
            existingTechnology.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingTechnology;
        }

        public async Task DeleteTechnologyAsync(string id)
        {
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