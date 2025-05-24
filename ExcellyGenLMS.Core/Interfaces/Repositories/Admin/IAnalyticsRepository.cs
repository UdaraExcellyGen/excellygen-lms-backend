// ExcellyGenLMS.Core/Interfaces/Repositories/Admin/IAnalyticsRepository.cs
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExcellyGenLMS.Core.Entities.Admin;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.Admin
{
    public interface IAnalyticsRepository
    {
        // Enrollment analytics
        Task<Dictionary<string, int>> GetEnrollmentsByCourseCategoryAsync(string categoryId);

        // Course availability
        Task<Dictionary<string, int>> GetCourseCountByCategoriesAsync();

        // User distribution
        Task<Dictionary<string, int>> GetUserCountByRolesAsync();

        // Get all course categories for dropdown
        Task<List<CourseCategory>> GetAllCourseCategoriesAsync();
    }
}