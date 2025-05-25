// ExcellyGenLMS.Core/Interfaces/Repositories/Course/IQuizRepository.cs
using ExcellyGenLMS.Core.Entities.Course;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.Course
{
    public interface IQuizRepository
    {
        // Quiz management
        Task<Quiz?> GetQuizByIdAsync(int quizId);
        Task<IEnumerable<Quiz>> GetQuizzesByLessonIdAsync(int lessonId);
        Task<Quiz?> GetQuizByLessonIdAsync(int lessonId); // Previously Added (single quiz for a lesson)
        Task<IEnumerable<Quiz>> GetQuizzesByLessonIdsAsync(List<int> lessonIds); // Previously Added (multiple quizzes by lesson IDs)
        Task<bool> HasQuizForLessonAsync(int lessonId); // Previously Added
        Task<Quiz> CreateQuizAsync(Quiz quiz);
        Task UpdateQuizAsync(Quiz quiz);
        Task DeleteQuizAsync(int quizId);

        // Quiz Bank operations
        Task<QuizBank?> GetQuizBankByIdAsync(int quizBankId);
        Task<QuizBank> CreateQuizBankAsync(QuizBank quizBank);
        Task<QuizBank> GetOrCreateQuizBankForLessonAsync(int lessonId);

        // Quiz Bank Question operations
        Task<QuizBankQuestion?> GetQuizBankQuestionByIdAsync(int questionId);
        Task<IEnumerable<QuizBankQuestion>> GetQuestionsForQuizBankAsync(int quizBankId);
        Task<QuizBankQuestion> AddQuestionToQuizBankAsync(QuizBankQuestion question);
        Task UpdateQuizBankQuestionAsync(QuizBankQuestion question);
        Task DeleteQuizBankQuestionAsync(int questionId);

        // MCQ Option operations
        Task<MCQQuestionOption?> GetMCQOptionByIdAsync(int optionId);
        Task<IEnumerable<MCQQuestionOption>> GetOptionsForQuestionAsync(int questionId);
        Task AddOptionToQuestionAsync(MCQQuestionOption option);
        Task UpdateOptionAsync(MCQQuestionOption option);
        Task DeleteOptionAsync(int optionId);

        // Random question selection for quiz attempts
        Task<IEnumerable<QuizBankQuestion>> GetRandomQuestionsForQuizAsync(int quizId, int count);
    }
}