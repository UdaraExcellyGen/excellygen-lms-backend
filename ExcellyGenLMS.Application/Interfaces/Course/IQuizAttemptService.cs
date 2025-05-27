// ExcellyGenLMS.Application/Interfaces/Course/IQuizAttemptService.cs
using ExcellyGenLMS.Application.DTOs.Course;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Interfaces.Course
{
    public interface IQuizAttemptService
    {
        Task<QuizAttemptDto> StartQuizAttemptAsync(string userId, int quizId);
        Task<bool> SubmitQuizAnswerAsync(SubmitQuizAnswerDto submitAnswerDto);
        Task<QuizAttemptDto> CompleteQuizAttemptAsync(CompleteQuizAttemptDto completeAttemptDto);
        Task<QuizAttemptDetailDto?> GetQuizAttemptDetailsAsync(int attemptId);
        Task<IEnumerable<QuizAttemptDto>> GetAttemptsByUserAsync(string userId);
        Task<IEnumerable<QuizAttemptDto>> GetAttemptsByQuizAsync(int quizId);
        Task<bool> HasUserCompletedQuizAsync(string userId, int quizId);
    }
}