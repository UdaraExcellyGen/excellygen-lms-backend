using ExcellyGenLMS.Core.Entities.Course;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.Course
{
    public interface IQuizAttemptRepository
    {
        Task<QuizAttempt?> GetQuizAttemptByIdAsync(int attemptId);
        Task<QuizAttempt?> GetActiveAttemptByUserAndQuizAsync(string userId, int quizId);
        Task<IEnumerable<QuizAttempt>> GetAttemptsByUserIdAsync(string userId);
        Task<IEnumerable<QuizAttempt>> GetAttemptsByQuizIdAsync(int quizId);
        Task<QuizAttempt> CreateQuizAttemptAsync(QuizAttempt attempt);
        Task UpdateQuizAttemptAsync(QuizAttempt attempt);
        Task DeleteQuizAttemptAsync(int attemptId);
        Task<IEnumerable<QuizAttempt>> GetCompletedAttemptsByUserAndQuizAsync(string userId, int quizId);
        Task<IEnumerable<QuizAttempt>> GetCompletedAttemptsByUserAndQuizzesAsync(string userId, List<int> quizIds);
        Task<IEnumerable<QuizAttempt>> GetCompletedAttemptsByQuizIdAsync(int quizId);
        Task<IEnumerable<QuizAttempt>> GetCompletedAttemptsByQuizIdsAsync(List<int> quizIds);
        Task<QuizAttemptAnswer?> GetAnswerByAttemptAndQuestionAsync(int attemptId, int questionId);
        Task<IEnumerable<QuizAttemptAnswer>> GetAnswersByAttemptIdAsync(int attemptId);
        Task<QuizAttemptAnswer> CreateQuizAttemptAnswerAsync(QuizAttemptAnswer answer);
        Task UpdateQuizAttemptAnswerAsync(QuizAttemptAnswer answer);
        Task DeleteQuizAttemptAnswerAsync(int answerId);
    }
}