// ExcellyGenLMS.Infrastructure/Data/Repositories/Course/QuizAttemptRepository.cs
using ExcellyGenLMS.Core.Entities.Course;
using ExcellyGenLMS.Core.Interfaces.Repositories.Course;
using ExcellyGenLMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Infrastructure.Data.Repositories.Course
{
    public class QuizAttemptRepository : IQuizAttemptRepository
    {
        private readonly ApplicationDbContext _context;

        public QuizAttemptRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<QuizAttempt?> GetQuizAttemptByIdAsync(int attemptId)
        {
            return await _context.QuizAttempts
                                 .Include(qa => qa.Quiz)
                                 .Include(qa => qa.User)
                                 .Include(qa => qa.Answers)
                                     .ThenInclude(a => a.Question)
                                         .ThenInclude(q => q!.MCQQuestionOptions)
                                 .Include(qa => qa.Answers)
                                     .ThenInclude(a => a.SelectedOption)
                                 .FirstOrDefaultAsync(qa => qa.QuizAttemptId == attemptId);
        }

        public async Task<QuizAttempt?> GetActiveAttemptByUserAndQuizAsync(string userId, int quizId)
        {
            return await _context.QuizAttempts
                                 .Include(qa => qa.Quiz)
                                 .Where(qa => qa.UserId == userId && qa.QuizId == quizId && !qa.IsCompleted)
                                 .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<QuizAttempt>> GetAttemptsByUserIdAsync(string userId)
        {
            return await _context.QuizAttempts
                                 .Include(qa => qa.Quiz)
                                 .Where(qa => qa.UserId == userId)
                                 .OrderByDescending(qa => qa.StartTime)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<QuizAttempt>> GetAttemptsByQuizIdAsync(int quizId)
        {
            return await _context.QuizAttempts
                                 .Include(qa => qa.User)
                                 .Include(qa => qa.Quiz)
                                 .Where(qa => qa.QuizId == quizId)
                                 .OrderByDescending(qa => qa.StartTime)
                                 .ToListAsync();
        }

        public async Task<QuizAttempt> CreateQuizAttemptAsync(QuizAttempt attempt)
        {
            _context.QuizAttempts.Add(attempt);
            await _context.SaveChangesAsync();

            // Return the attempt with loaded navigation properties
            return await GetQuizAttemptByIdAsync(attempt.QuizAttemptId) ?? attempt;
        }

        public async Task UpdateQuizAttemptAsync(QuizAttempt attempt)
        {
            _context.Entry(attempt).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteQuizAttemptAsync(int attemptId)
        {
            var attempt = await _context.QuizAttempts.FindAsync(attemptId);
            if (attempt != null)
            {
                _context.QuizAttempts.Remove(attempt);
                await _context.SaveChangesAsync();
            }
        }

        // Answer operations
        public async Task<QuizAttemptAnswer?> GetAnswerByAttemptAndQuestionAsync(int attemptId, int questionId)
        {
            return await _context.QuizAttemptAnswers
                                 .Include(qaa => qaa.SelectedOption)
                                 .Include(qaa => qaa.Question)
                                     .ThenInclude(q => q!.MCQQuestionOptions)
                                 .FirstOrDefaultAsync(qaa => qaa.QuizAttemptId == attemptId && qaa.QuizBankQuestionId == questionId);
        }

        public async Task<IEnumerable<QuizAttemptAnswer>> GetAnswersByAttemptIdAsync(int attemptId)
        {
            return await _context.QuizAttemptAnswers
                                 .Include(qaa => qaa.Question)
                                     .ThenInclude(q => q!.MCQQuestionOptions)
                                 .Include(qaa => qaa.SelectedOption)
                                 .Where(qaa => qaa.QuizAttemptId == attemptId)
                                 .OrderBy(qaa => qaa.Question!.QuestionBankOrder)
                                 .ToListAsync();
        }

        public async Task<QuizAttemptAnswer> CreateQuizAttemptAnswerAsync(QuizAttemptAnswer answer)
        {
            _context.QuizAttemptAnswers.Add(answer);
            await _context.SaveChangesAsync();
            return answer;
        }

        public async Task UpdateQuizAttemptAnswerAsync(QuizAttemptAnswer answer)
        {
            _context.Entry(answer).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<QuizAttempt>> GetCompletedAttemptsByUserAndQuizAsync(string userId, int quizId)
        {
            return await _context.QuizAttempts
                .Where(qa => qa.UserId == userId &&
                             qa.QuizId == quizId &&
                             qa.IsCompleted == true)
                .ToListAsync();
        }

        public async Task DeleteQuizAttemptAnswerAsync(int answerId)
        {
            var answer = await _context.QuizAttemptAnswers.FindAsync(answerId);
            if (answer != null)
            {
                _context.QuizAttemptAnswers.Remove(answer);
                await _context.SaveChangesAsync();
            }
        }
    }
}