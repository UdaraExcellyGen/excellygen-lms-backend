using System.Collections.Generic;
using System.Threading.Tasks;
using ExcellyGenLMS.Application.DTOs.Course;

namespace ExcellyGenLMS.Application.Interfaces.Course
{
    public interface ICourseCoordinatorAnalyticsService
    {
        Task<EnrollmentAnalyticsResponse> GetEnrollmentAnalyticsAsync(string coordinatorId, string? categoryId, string status, string ownership);
        Task<List<CourseCategoryAnalyticsDto>> GetCourseCategoriesAsync(string coordinatorId);
        Task<List<CoordinatorCourseAnalyticsDto>> GetCoordinatorCoursesAsync(string coordinatorId, string? categoryId, string ownership);
        Task<List<CourseQuizAnalyticsDto>> GetQuizzesForCourseAsync(int courseId, string coordinatorId);
        Task<QuizPerformanceAnalyticsResponse> GetQuizPerformanceAsync(int quizId, string coordinatorId);
    }
}