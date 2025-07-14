using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExcellyGenLMS.Core.Interfaces.Repositories.Learner;
using Microsoft.EntityFrameworkCore;
using System;

namespace ExcellyGenLMS.Infrastructure.Data.Repositories.Learner
{
    public class LeaderboardRepository : ILeaderboardRepository
    {
        private readonly ApplicationDbContext _context;
        private const int CourseCompletionPoints = 100;
        // THIS IS THE FINAL FIX: We standardize the maximum points for any quiz.
        private const int QuizMaxPoints = 100;

        public LeaderboardRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserPoints>> GetUserPointsAsync()
        {
            // Part 1: Course completion points (This logic is correct)
            var coursePoints = _context.Enrollments
                .Where(e => e.Status == "completed")
                .GroupBy(e => e.UserId)
                .Select(g => new { UserId = g.Key, Points = g.Count() * CourseCompletionPoints });

            // Part 2: Quiz points calculation (DEFINITIVELY CORRECTED)
            var quizPoints = _context.QuizAttempts
                // Find all completed attempts that have questions
                .Where(qa => qa.IsCompleted && qa.TotalQuestions > 0)
                .Select(attempt => new
                {
                    attempt.UserId,
                    // Calculate points based on a standard 100-point scale,
                    // completely ignoring the incorrect TotalMarks value from the database.
                    Points = (int)Math.Round(
                        // Calculate the percentage score (e.g., 0.5 for 1 out of 2)
                        ((double)attempt.CorrectAnswers / attempt.TotalQuestions)
                        // Multiply by our standard max points.
                        * QuizMaxPoints
                    )
                })
                // Group the calculated points by user
                .GroupBy(q => q.UserId)
                // Sum up all the quiz points for each user
                .Select(g => new
                {
                    UserId = g.Key,
                    Points = g.Sum(q => q.Points)
                });

            // Part 3: Combine course points and quiz points (This logic is correct)
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