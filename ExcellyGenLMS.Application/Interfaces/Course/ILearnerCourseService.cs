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

        // MODIFIED: The old MarkLessonCompletedAsync is removed.
        // This is the only method that should be here for marking progress.
        Task<DocumentProgressDto> MarkDocumentCompletedAsync(string userId, int documentId);

        Task<bool> HasLearnerCompletedAllCourseContentAsync(string userId, int courseId);
        Task<LearnerCourseDto?> GetCoursePreviewAsync(string userId, int courseId);
    }
}