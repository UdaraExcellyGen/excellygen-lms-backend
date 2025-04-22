using ExcellyGenLMS.Core.Entities.Admin;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.Admin
{
    public interface ICourseCategoryRepository
    {
        Task<List<CourseCategory>> GetAllCategoriesAsync();
        Task<CourseCategory?> GetCategoryByIdAsync(string id);
        Task<CourseCategory> CreateCategoryAsync(CourseCategory category);
        Task<CourseCategory> UpdateCategoryAsync(CourseCategory category);
        Task DeleteCategoryAsync(string id);
        Task<CourseCategory> ToggleCategoryStatusAsync(string id);
        Task<int> GetCoursesCountByCategoryIdAsync(string categoryId);
    }
}