// ExcellyGenLMS.Core/Interfaces/Repositories/Course/ILessonProgressRepository.cs
using ExcellyGenLMS.Core.Entities.Course;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.Course
{
    public interface ILessonProgressRepository
    {
        Task<LessonProgress?> GetByIdAsync(int id);
        Task<LessonProgress?> GetProgressByUserIdAndLessonIdAsync(string userId, int lessonId);
        Task<IEnumerable<LessonProgress>> GetProgressByUserIdAndCourseIdAsync(string userId, int courseId);
        Task<LessonProgress> AddAsync(LessonProgress progress);
        Task UpdateAsync(LessonProgress progress);
        Task DeleteAsync(int id);
    }
}