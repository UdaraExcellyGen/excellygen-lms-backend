<<<<<<< HEAD
//using ExcellyGenLMS.Core.Entities.Course;

//namespace ExcellyGenLMS.Core.Interfaces.Repositories.Admin
//{
//    public interface ICourseAdminRepository
//    {
//        Task<List<Course>> GetCoursesByCategoryIdAsync(string categoryId);
//        Task<Course?> GetCourseByIdAsync(int id);
//        Task<Course> UpdateCourseAsync(Course course);
//        Task DeleteCourseAsync(int id);
//    }
//}

// Note: You might not actually NEED this ICourseAdminRepository if all course logic
// is handled by the CourseCoordinator through ICourseRepository.
// Review if this interface is genuinely required for the Admin role specifically.
// If not, you might delete this file entirely. Assuming it IS needed for now:

// using ExcellyGenLMS.Core.Entities.Course; // <-- Keep this using if you prefer aliasing below, OR remove if using FQN everywhere
using System.Collections.Generic;
using System.Threading.Tasks;

// Add an alias to avoid conflict if desired (alternative to FQN)
// using CourseEntity = ExcellyGenLMS.Core.Entities.Course.Course;
=======
using System.Collections.Generic;
using System.Threading.Tasks;
>>>>>>> 1d0db7143db1398cf4c8b7ab2577208b67f84a93

namespace ExcellyGenLMS.Core.Interfaces.Repositories.Admin
{
    /// <summary>
    /// Interface specific to Admin operations related to Courses (if different from Coordinator).
    /// Review if this overlaps too much with ICourseRepository.
    /// </summary>
    public interface ICourseAdminRepository
    {
<<<<<<< HEAD
        /// <summary>
        /// Gets courses filtered by category ID (example admin function).
        /// </summary>
        Task<List<ExcellyGenLMS.Core.Entities.Course.Course>> GetCoursesByCategoryIdAsync(string categoryId);
        // Or using alias: Task<List<CourseEntity>> GetCoursesByCategoryIdAsync(string categoryId);

        /// <summary>
        /// Gets a specific course by ID (potentially with different includes than coordinator view).
        /// </summary>
        Task<ExcellyGenLMS.Core.Entities.Course.Course?> GetCourseByIdAsync(int id);
        // Or using alias: Task<CourseEntity?> GetCourseByIdAsync(int id);

        /// <summary>
        /// Updates a course (potentially different permissions/fields than coordinator update).
        /// </summary>
        Task<ExcellyGenLMS.Core.Entities.Course.Course> UpdateCourseAsync(ExcellyGenLMS.Core.Entities.Course.Course course);
        // Or using alias: Task<CourseEntity> UpdateCourseAsync(CourseEntity course);

        /// <summary>
        /// Deletes a course (potentially different logic/checks than coordinator delete).
        /// </summary>
=======
        Task<List<ExcellyGenLMS.Core.Entities.Course.Course>> GetCoursesByCategoryIdAsync(string categoryId);
        Task<ExcellyGenLMS.Core.Entities.Course.Course?> GetCourseByIdAsync(int id);
        Task<ExcellyGenLMS.Core.Entities.Course.Course> UpdateCourseAsync(ExcellyGenLMS.Core.Entities.Course.Course course);
>>>>>>> 1d0db7143db1398cf4c8b7ab2577208b67f84a93
        Task DeleteCourseAsync(int id);
    }
}