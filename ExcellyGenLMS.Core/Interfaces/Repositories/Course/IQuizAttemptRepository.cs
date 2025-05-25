// ExcellyGenLMS.Core/Interfaces/Repositories/Course/IQuizAttemptRepository.cs
using ExcellyGenLMS.Core.Entities.Course;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.Course
{
    public interface IQuizAttemptRepository
    {
        Task<QuizAttempt?> GetQuizAttemptByIdAsync(int attemptId);
        Task<IEnumerable<QuizAttempt>> GetAttemptsByUserIdAsync(string userId);
        Task<IEnumerable<QuizAttempt>> GetAttemptsByQuizIdAsync(int quizId);
        Task<IEnumerable<QuizAttempt>> GetAttemptsByUserAndQuizAsync(string userId, int quizId);
        Task<QuizAttempt> CreateQuizAttemptAsync(QuizAttempt attempt);
        Task UpdateQuizAttemptAsync(QuizAttempt attempt);

        Task<QuizAttemptAnswer?> GetQuizAttemptAnswerByIdAsync(int answerId);
        Task<IEnumerable<QuizAttemptAnswer>> GetAnswersByAttemptIdAsync(int attemptId);
        Task<QuizAttemptAnswer> AddAnswerToAttemptAsync(QuizAttemptAnswer answer);
        Task UpdateQuizAttemptAnswerAsync(QuizAttemptAnswer answer);
    }
}