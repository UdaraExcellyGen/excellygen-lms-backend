using ExcellyGenLMS.Core.Entities.Course;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.Course
{
    public interface IQuizRepository
    {
        // Quiz operations
        Task<Quiz?> GetQuizByIdAsync(int quizId);
        Task<IEnumerable<Quiz>> GetQuizzesByLessonIdAsync(int lessonId);
        Task<IEnumerable<Quiz>> GetQuizzesByCourseIdAsync(int courseId); // This method is needed
        Task<Quiz?> GetQuizByLessonIdAsync(int lessonId);
        Task<IEnumerable<Quiz>> GetQuizzesByLessonIdsAsync(List<int> lessonIds);
        Task<Quiz> CreateQuizAsync(Quiz quiz);
        Task UpdateQuizAsync(Quiz quiz);
        Task DeleteQuizAsync(int quizId);
        Task<bool> HasQuizForLessonAsync(int lessonId);

        // Quiz Bank operations
        Task<QuizBank?> GetQuizBankByIdAsync(int quizBankId);
        Task<QuizBank> CreateQuizBankAsync(QuizBank quizBank);
        Task UpdateQuizBankAsync(QuizBank quizBank);
        Task<QuizBank> GetOrCreateQuizBankForLessonAsync(int lessonId);

        // Question operations
        Task<QuizBankQuestion?> GetQuizBankQuestionByIdAsync(int questionId);
        Task<IEnumerable<QuizBankQuestion>> GetQuestionsForQuizBankAsync(int quizBankId);
        Task<QuizBankQuestion> AddQuestionToQuizBankAsync(QuizBankQuestion question);
        Task UpdateQuizBankQuestionAsync(QuizBankQuestion question);
        Task DeleteQuizBankQuestionAsync(int questionId);
        Task<IEnumerable<QuizBankQuestion>> GetRandomQuestionsForQuizAsync(int quizId, int count);

        // MCQ Option operations
        Task<MCQQuestionOption?> GetMCQOptionByIdAsync(int optionId);
        Task<IEnumerable<MCQQuestionOption>> GetOptionsForQuestionAsync(int questionId);
        Task AddOptionToQuestionAsync(MCQQuestionOption option);
        Task UpdateOptionAsync(MCQQuestionOption option);
        Task DeleteOptionAsync(int optionId);
        Task<bool> IsOptionUsedInAttemptsAsync(int optionId);
    }
}