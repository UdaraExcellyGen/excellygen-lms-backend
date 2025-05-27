using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExcellyGenLMS.Core.Entities.Admin;
using ExcellyGenLMS.Core.Entities.Learner;
using ExcellyGenLMS.Core.Interfaces.Repositories.Learner;
using ExcellyGenLMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ExcellyGenLMS.Infrastructure.Data.Repositories.Learner
{
    public class UserTechnologyRepository : IUserTechnologyRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserTechnologyRepository> _logger;

        public UserTechnologyRepository(
            ApplicationDbContext context,
            ILogger<UserTechnologyRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<UserTechnology>> GetUserTechnologiesAsync(string userId)
        {
            return await _context.UserTechnologies
                .Include(ut => ut.Technology)
                .Where(ut => ut.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<Technology>> GetAvailableTechnologiesAsync(string userId)
        {
            // Get IDs of technologies the user already has
            var userTechIds = await _context.UserTechnologies
                .Where(ut => ut.UserId == userId)
                .Select(ut => ut.TechnologyId)
                .ToListAsync();

            // Get technologies the user doesn't have yet
            return await _context.Technologies
                .Where(t => !userTechIds.Contains(t.Id) && t.Status == "active")
                .ToListAsync();
        }

        public async Task<UserTechnology> AddUserTechnologyAsync(string userId, string technologyId)
        {
            // Check if user already has this technology
            var existingUserTech = await _context.UserTechnologies
                .Include(ut => ut.Technology)
                .FirstOrDefaultAsync(ut => ut.UserId == userId && ut.TechnologyId == technologyId);

            if (existingUserTech != null)
            {
                return existingUserTech;
            }

            // Check if technology exists
            var technology = await _context.Technologies.FindAsync(technologyId)
                ?? throw new KeyNotFoundException($"Technology with ID {technologyId} not found");

            var userTechnology = new UserTechnology
            {
                UserId = userId,
                TechnologyId = technologyId,
                AddedDate = DateTime.UtcNow
            };

            _context.UserTechnologies.Add(userTechnology);
            await _context.SaveChangesAsync();

            // Fetch the complete entity with Technology included
            return await _context.UserTechnologies
                .Include(ut => ut.Technology)
                .FirstAsync(ut => ut.Id == userTechnology.Id);
        }

        public async Task RemoveUserTechnologyAsync(string userId, string technologyId)
        {
            var userTechnology = await _context.UserTechnologies
                .FirstOrDefaultAsync(ut => ut.UserId == userId && ut.TechnologyId == technologyId)
                ?? throw new KeyNotFoundException($"Technology with ID {technologyId} not found for user with ID {userId}");

            _context.UserTechnologies.Remove(userTechnology);
            await _context.SaveChangesAsync();
        }
    }
}