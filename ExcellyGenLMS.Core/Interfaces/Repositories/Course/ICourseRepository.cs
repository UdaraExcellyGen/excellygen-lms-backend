// ExcellyGenLMS.Core/Interfaces/Repositories/Course/ICourseRepository.cs
using ExcellyGenLMS.Core.Entities.Course;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.Course
{
    public interface ICourseRepository
    {
        Task<Core.Entities.Course.Course?> GetByIdAsync(int id);
        Task<Core.Entities.Course.Course?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<Core.Entities.Course.Course>> GetAllAsync();
        Task<IEnumerable<Core.Entities.Course.Course>> GetAllPublishedCoursesWithDetailsAsync();
        Task<IEnumerable<Lesson>> GetLessonsByCourseIdAsync(int courseId);
        Task<IEnumerable<Lesson>> GetLessonsByCourseIdsAsync(List<int> courseIds);
        Task<Lesson?> GetLessonWithDocumentsAsync(int lessonId);
        Task<Core.Entities.Course.Course> AddAsync(Core.Entities.Course.Course course);
        Task UpdateAsync(Core.Entities.Course.Course course);
        Task DeleteAsync(int id);
        Task AddTechnologyAsync(int courseId, string technologyId);
        Task RemoveTechnologyAsync(int courseId, string technologyId);
        Task<IEnumerable<CourseTechnology>> GetCourseTechnologiesAsync(int courseId);
        Task ClearTechnologiesAsync(int courseId);
        Task<int> GetTotalPublishedCoursesCountAsync();
        Task<TimeSpan?> GetOverallAverageCourseDurationAsync();

        // NEW METHODS FOR ANALYTICS
        Task<IEnumerable<Core.Entities.Course.Course>> GetCoursesByCreatorIdAsync(string creatorId);
        Task<IEnumerable<Core.Entities.Course.Course>> GetCoursesByCreatorIdAndCategoryAsync(string creatorId, string categoryId);
    }
}