using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.Admin
{
    /// <summary>
    /// Interface specific to Admin operations related to Courses (if different from Coordinator).
    /// </summary>
    public interface ICourseAdminRepository
    {
        /// <summary>
        /// Gets courses filtered by category ID (example admin function).
        /// </summary>
        Task<List<ExcellyGenLMS.Core.Entities.Course.Course>> GetCoursesByCategoryIdAsync(string categoryId);

        /// <summary>
        /// Gets a specific course by ID (potentially with different includes than coordinator view).
        /// </summary>
        Task<ExcellyGenLMS.Core.Entities.Course.Course?> GetCourseByIdAsync(int id);

        /// <summary>
        /// Updates a course (potentially different permissions/fields than coordinator update).
        /// </summary>
        Task<ExcellyGenLMS.Core.Entities.Course.Course> UpdateCourseAsync(ExcellyGenLMS.Core.Entities.Course.Course course);

        /// <summary>
        /// Deletes a course (potentially different logic/checks than coordinator delete).
        /// </summary>
        Task DeleteCourseAsync(int id);
    }
}