// ExcellyGenLMS.Application/Interfaces/Course/ICourseCoordinatorAnalyticsService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using ExcellyGenLMS.Application.DTOs.Course;

namespace ExcellyGenLMS.Application.Interfaces.Course
{
    public interface ICourseCoordinatorAnalyticsService
    {
        // Enhanced methods that replace the old ones
        Task<EnrollmentAnalyticsResponse> GetEnrollmentAnalyticsAsync(string coordinatorId, string? categoryId = null, string status = "all");
        Task<List<CourseCategoryAnalyticsDto>> GetCourseCategoriesAsync(string coordinatorId);
        Task<List<CoordinatorCourseAnalyticsDto>> GetCoordinatorCoursesAsync(string coordinatorId, string? categoryId = null);
        Task<List<CourseQuizAnalyticsDto>> GetQuizzesForCourseAsync(int courseId, string coordinatorId);
        Task<QuizPerformanceAnalyticsResponse> GetQuizPerformanceAsync(int quizId, string coordinatorId);

        // Keep original method signatures for backward compatibility
        Task<IEnumerable<CourseEnrollmentAnalyticsDto>> GetEnrollmentAnalyticsSimpleAsync(string coordinatorId);
        Task<IEnumerable<CoordinatorCourseDto>> GetCoordinatorCoursesSimpleAsync(string coordinatorId);
        Task<IEnumerable<CourseQuizDto>> GetQuizzesForCourseSimpleAsync(int courseId, string coordinatorId);
        Task<IEnumerable<MarkRangeDataDto>> GetQuizPerformanceSimpleAsync(int quizId, string coordinatorId);
    }
}