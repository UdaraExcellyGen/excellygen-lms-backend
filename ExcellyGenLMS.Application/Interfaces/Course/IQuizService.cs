// ExcellyGenLMS.Application/Interfaces/Course/IQuizService.cs
using ExcellyGenLMS.Application.DTOs.Course;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Interfaces.Course
{
    public interface IQuizService
    {
        // Quiz management
        Task<QuizDto?> GetQuizByIdAsync(int quizId);
        Task<QuizDetailDto?> GetQuizDetailsAsync(int quizId);
        Task<IEnumerable<QuizDto>> GetQuizzesByLessonIdAsync(int lessonId);
        Task<QuizDto> CreateQuizAsync(CreateQuizDto createQuizDto);
        Task UpdateQuizAsync(int quizId, CreateQuizDto updateQuizDto);
        Task DeleteQuizAsync(int quizId);

        // Quiz Bank management
        Task<QuizBankDto?> GetQuizBankByIdAsync(int quizBankId);
        Task<QuizBankDto> CreateQuizBankAsync(int lessonId, CreateQuizBankDto createQuizBankDto);

        // Question management
        Task<QuizBankQuestionDto?> GetQuestionByIdAsync(int questionId);
        Task<IEnumerable<QuizBankQuestionDto>> GetQuestionsForQuizBankAsync(int quizBankId);
        Task<QuizBankQuestionDto> AddQuestionToQuizBankAsync(int quizBankId, CreateQuizBankQuestionDto createQuestionDto);
        Task UpdateQuizBankQuestionAsync(int questionId, UpdateQuizBankQuestionDto updateQuestionDto);
        Task DeleteQuizBankQuestionAsync(int questionId);

        // Learner-facing quiz methods
        Task<IEnumerable<LearnerQuizQuestionDto>> GetQuestionsForLearnerQuizAsync(int quizId);

        // Course-level quiz methods
        Task<IEnumerable<QuizDto>> GetQuizzesByCourseIdAsync(int courseId);
    }
}