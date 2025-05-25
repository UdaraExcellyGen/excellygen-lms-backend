// ExcellyGenLMS.Infrastructure/Data/Repositories/Course/QuizAttemptRepository.cs
using ExcellyGenLMS.Core.Entities.Course;
using ExcellyGenLMS.Core.Interfaces.Repositories.Course;
using ExcellyGenLMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Infrastructure.Data.Repositories.Course
{
    public class QuizAttemptRepository : IQuizAttemptRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<QuizAttemptRepository> _logger;

        public QuizAttemptRepository(ApplicationDbContext context, ILogger<QuizAttemptRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<QuizAttempt?> GetQuizAttemptByIdAsync(int attemptId)
        {
            return await _context.Set<QuizAttempt>()
                .Include(a => a.Quiz)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.QuizAttemptId == attemptId);
        }

        public async Task<IEnumerable<QuizAttempt>> GetAttemptsByUserIdAsync(string userId)
        {
            return await _context.Set<QuizAttempt>()
                .Where(a => a.UserId == userId)
                .Include(a => a.Quiz)
                .OrderByDescending(a => a.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<QuizAttempt>> GetAttemptsByQuizIdAsync(int quizId)
        {
            return await _context.Set<QuizAttempt>()
                .Where(a => a.QuizId == quizId)
                .Include(a => a.User)
                .OrderByDescending(a => a.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<QuizAttempt>> GetAttemptsByUserAndQuizAsync(string userId, int quizId)
        {
            return await _context.Set<QuizAttempt>()
                .Where(a => a.UserId == userId && a.QuizId == quizId)
                .Include(a => a.Quiz)
                .OrderByDescending(a => a.StartTime)
                .ToListAsync();
        }

        public async Task<QuizAttempt> CreateQuizAttemptAsync(QuizAttempt attempt)
        {
            _context.Set<QuizAttempt>().Add(attempt);
            await _context.SaveChangesAsync();
            return attempt;
        }

        public async Task UpdateQuizAttemptAsync(QuizAttempt attempt)
        {
            _context.Entry(attempt).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<QuizAttemptAnswer?> GetQuizAttemptAnswerByIdAsync(int answerId)
        {
            return await _context.Set<QuizAttemptAnswer>()
                .Include(a => a.Question)
                .Include(a => a.SelectedOption)
                .FirstOrDefaultAsync(a => a.QuizAttemptAnswerId == answerId);
        }

        public async Task<IEnumerable<QuizAttemptAnswer>> GetAnswersByAttemptIdAsync(int attemptId)
        {
            return await _context.Set<QuizAttemptAnswer>()
                .Where(a => a.QuizAttemptId == attemptId)
                .Include(a => a.Question)
                .Include(a => a.SelectedOption)
                .ToListAsync();
        }

        public async Task<QuizAttemptAnswer> AddAnswerToAttemptAsync(QuizAttemptAnswer answer)
        {
            _context.Set<QuizAttemptAnswer>().Add(answer);
            await _context.SaveChangesAsync();
            return answer;
        }

        public async Task UpdateQuizAttemptAnswerAsync(QuizAttemptAnswer answer)
        {
            _context.Entry(answer).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}