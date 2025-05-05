using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExcellyGenLMS.Core.Entities.Learner;
using ExcellyGenLMS.Core.Interfaces.Repositories.Learner;
using ExcellyGenLMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ExcellyGenLMS.Infrastructure.Data.Repositories.Learner
{
    public class UserProjectRepository : IUserProjectRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserProjectRepository> _logger;

        public UserProjectRepository(
            ApplicationDbContext context,
            ILogger<UserProjectRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Project>> GetUserProjectsAsync(string userId)
        {
            return await _context.UserProjects
                .Include(up => up.Project)
                    .ThenInclude(p => p.Technologies)
                        .ThenInclude(pt => pt.Technology)
                .Where(up => up.UserId == userId)
                .Select(up => up.Project)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();
        }

        public async Task<Project> GetUserProjectByIdAsync(string userId, string projectId)
        {
            var userProject = await _context.UserProjects
                .Include(up => up.Project)
                    .ThenInclude(p => p.Technologies)
                        .ThenInclude(pt => pt.Technology)
                .FirstOrDefaultAsync(up => up.UserId == userId && up.ProjectId == projectId);

            return userProject?.Project
                ?? throw new KeyNotFoundException($"Project with ID {projectId} not found for user with ID {userId}");
        }
    }
}