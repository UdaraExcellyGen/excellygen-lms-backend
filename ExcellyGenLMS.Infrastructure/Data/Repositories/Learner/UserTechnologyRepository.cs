// Path: ExcellyGenLMS.Infrastructure/Data/Repositories/Learner/UserTechnologyRepository.cs

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

        // NEW OPTIMIZED METHOD: Bulk load skills for multiple users to fix N+1 query problem
        public async Task<Dictionary<string, List<string>>> GetSkillsForMultipleUsersAsync(List<string> userIds)
        {
            try
            {
                if (userIds == null || !userIds.Any())
                {
                    return new Dictionary<string, List<string>>();
                }

                _logger.LogInformation($"Loading skills for {userIds.Count} users in bulk");

                var userTechnologies = await _context.UserTechnologies
                    .Include(ut => ut.Technology)
                    .Where(ut => userIds.Contains(ut.UserId))
                    .Select(ut => new { ut.UserId, TechnologyName = ut.Technology!.Name })
                    .ToListAsync();

                var skillsDict = userIds.ToDictionary(
                    userId => userId,
                    userId => userTechnologies
                        .Where(ut => ut.UserId == userId)
                        .Select(ut => ut.TechnologyName)
                        .ToList()
                );

                _logger.LogInformation($"Loaded skills for {skillsDict.Keys.Count} users with {userTechnologies.Count} total skills");
                return skillsDict;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading skills for multiple users: {string.Join(", ", userIds ?? new List<string>())}");
                // Return empty dictionary instead of throwing to prevent breaking the employee loading
                return userIds?.ToDictionary(id => id, id => new List<string>()) ?? new Dictionary<string, List<string>>();
            }
        }

        // NEW METHOD: Get user technologies count for multiple users (for statistics)
        public async Task<Dictionary<string, int>> GetTechnologyCountForMultipleUsersAsync(List<string> userIds)
        {
            try
            {
                if (userIds == null || !userIds.Any())
                {
                    return new Dictionary<string, int>();
                }

                var counts = await _context.UserTechnologies
                    .Where(ut => userIds.Contains(ut.UserId))
                    .GroupBy(ut => ut.UserId)
                    .Select(g => new { UserId = g.Key, Count = g.Count() })
                    .ToListAsync();

                return userIds.ToDictionary(
                    userId => userId,
                    userId => counts.FirstOrDefault(c => c.UserId == userId)?.Count ?? 0
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Error getting technology counts for users: {string.Join(", ", userIds ?? new List<string>())}");
                return userIds?.ToDictionary(id => id, id => 0) ?? new Dictionary<string, int>();
            }
        }
    }
}