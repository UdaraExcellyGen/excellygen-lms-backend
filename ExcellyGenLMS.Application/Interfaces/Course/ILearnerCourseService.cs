// ExcellyGenLMS.Application/Interfaces/Course/ILearnerCourseService.cs
using ExcellyGenLMS.Application.DTOs.Course;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Interfaces.Course
{
    public interface ILearnerCourseService
    {
        Task<IEnumerable<LearnerCourseDto>> GetAvailableCoursesAsync(string userId, string? categoryId = null);
        Task<IEnumerable<LearnerCourseDto>> GetEnrolledCoursesAsync(string userId);
        Task<LearnerCourseDto?> GetLearnerCourseDetailsAsync(string userId, int courseId);
        Task<LessonProgressDto> MarkLessonCompletedAsync(string userId, int lessonId);
        Task<bool> HasLearnerCompletedAllCourseContentAsync(string userId, int courseId);
    }
}