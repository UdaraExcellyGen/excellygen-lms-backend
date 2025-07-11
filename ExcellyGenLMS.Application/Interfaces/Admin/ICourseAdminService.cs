using ExcellyGenLMS.Application.DTOs.Course;
using ExcellyGenLMS.Application.DTOs.Admin;

namespace ExcellyGenLMS.Application.Interfaces.Admin
{
    public interface ICourseAdminService
    {
        // Existing methods
        Task<List<CourseDto>> GetCoursesByCategoryIdAsync(string categoryId);
        Task<CourseDto> UpdateCourseAdminAsync(int id, UpdateCourseAdminDto updateCourseDto);
        Task DeleteCourseAsync(int id);

        // OPTIMIZATION: New method for getting category statistics
        // Using the full namespace to avoid ambiguity
        Task<ExcellyGenLMS.Application.Services.Admin.AdminCategoryStatsDto> GetCategoryStatsAsync(string categoryId);
    }
}