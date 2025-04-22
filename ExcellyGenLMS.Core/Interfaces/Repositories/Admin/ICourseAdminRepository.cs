using ExcellyGenLMS.Core.Entities.Course;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.Admin
{
    public interface ICourseAdminRepository
    {
        Task<List<Course>> GetCoursesByCategoryIdAsync(string categoryId);
        Task<Course?> GetCourseByIdAsync(int id);
        Task<Course> UpdateCourseAsync(Course course);
        Task DeleteCourseAsync(int id);
    }
}