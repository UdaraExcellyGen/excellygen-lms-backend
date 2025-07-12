// ExcellyGenLMS.Application/Interfaces/course/ICourseCoordinatorAnalyticsService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using ExcellyGenLMS.Application.DTOs.Course;

namespace ExcellyGenLMS.Application.Interfaces.Course
{
    public interface ICourseCoordinatorAnalyticsService
    {
        Task<IEnumerable<CourseEnrollmentAnalyticsDto>> GetEnrollmentAnalyticsAsync(string coordinatorId);
        Task<IEnumerable<CoordinatorCourseDto>> GetCoordinatorCoursesAsync(string coordinatorId);
        Task<IEnumerable<CourseQuizDto>> GetQuizzesForCourseAsync(int courseId, string coordinatorId);
        Task<IEnumerable<MarkRangeDataDto>> GetQuizPerformanceAsync(int quizId, string coordinatorId);
    }
}