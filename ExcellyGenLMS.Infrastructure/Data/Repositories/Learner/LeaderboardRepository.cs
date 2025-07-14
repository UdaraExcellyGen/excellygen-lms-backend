using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExcellyGenLMS.Core.Interfaces.Repositories.Learner;
using Microsoft.EntityFrameworkCore;

namespace ExcellyGenLMS.Infrastructure.Data.Repositories.Learner
{
    public class LeaderboardRepository : ILeaderboardRepository
    {
        private readonly ApplicationDbContext _context;
        private const int CourseCompletionPoints = 100;

        public LeaderboardRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserPoints>> GetUserPointsAsync()
        {
            var coursePoints = _context.Enrollments
                .Where(e => e.Status == "completed")
                .GroupBy(e => e.UserId)
                .Select(g => new { UserId = g.Key, Points = g.Count() * CourseCompletionPoints });

            var quizPoints = _context.QuizAttempts
                .Where(qa => qa.IsCompleted && qa.Score.HasValue)
                .GroupBy(qa => qa.UserId)
                // FIX: Using ?? 0 to safely handle nullable Score, resolving the warning.
                .Select(g => new { UserId = g.Key, Points = g.Sum(qa => qa.Score ?? 0) });

            var allPoints = await coursePoints
                .Concat(quizPoints)
                .ToListAsync();

            var userPoints = allPoints
                .GroupBy(p => p.UserId)
                .Select(g => new UserPoints
                {
                    UserId = g.Key,
                    TotalPoints = g.Sum(p => p.Points)
                })
                .ToList();

            return userPoints;
        }

        public async Task<IEnumerable<UserLeaderboardInfo>> GetUsersByIdsAsync(IEnumerable<string> userIds)
        {
            return await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new UserLeaderboardInfo
                {
                    Id = u.Id,
                    Name = u.Name,
                    Avatar = u.Avatar,
                    JobRole = u.JobRole
                })
                .ToListAsync();
        }
    }
}