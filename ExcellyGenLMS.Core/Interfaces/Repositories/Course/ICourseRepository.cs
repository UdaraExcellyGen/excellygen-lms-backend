using ExcellyGenLMS.Core.Entities.Course;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.Course
{
    /// <summary>
    /// Interface for repository handling Course entity operations.
    /// </summary>
    public interface ICourseRepository
    {
        /// <summary>
        /// Gets a Course by its ID.
        /// </summary>
        /// <param name="id">The course ID.</param>
        /// <returns>The Course entity or null if not found.</returns>
        Task<Core.Entities.Course.Course?> GetByIdAsync(int id);

        /// <summary>
        /// Gets a Course by its ID, including related details like Lessons, Documents, Category, Creator, and Technologies.
        /// </summary>
        /// <param name="id">The course ID.</param>
        /// <returns>The Course entity with related data or null if not found.</returns>
        Task<Core.Entities.Course.Course?> GetByIdWithDetailsAsync(int id);

        /// <summary>
        /// Gets all Course entities. Consider adding filtering, sorting, and pagination parameters for production use.
        /// </summary>
        /// <returns>An enumerable collection of Course entities.</returns>
        Task<IEnumerable<Core.Entities.Course.Course>> GetAllAsync();

        /// <summary>
        /// Adds a new Course entity to the database.
        /// </summary>
        /// <param name="course">The course entity to add.</param>
        /// <returns>The added Course entity (potentially with DB-generated ID).</returns>
        Task<Core.Entities.Course.Course> AddAsync(Core.Entities.Course.Course course);

        /// <summary>
        /// Updates an existing Course entity in the database.
        /// </summary>
        /// <param name="course">The course entity with updated values.</param>
        Task UpdateAsync(Core.Entities.Course.Course course);

        /// <summary>
        /// Deletes a Course entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the course to delete.</param>
        Task DeleteAsync(int id);

        /// <summary>
        /// Creates an association between a course and a technology.
        /// </summary>
        /// <param name="courseId">The ID of the course.</param>
        /// <param name="technologyId">The ID of the technology.</param>
        Task AddTechnologyAsync(int courseId, string technologyId);

        /// <summary>
        /// Removes an association between a course and a technology.
        /// </summary>
        /// <param name="courseId">The ID of the course.</param>
        /// <param name="technologyId">The ID of the technology.</param>
        Task RemoveTechnologyAsync(int courseId, string technologyId);

        /// <summary>
        /// Gets all technology associations for a specific course.
        /// </summary>
        /// <param name="courseId">The ID of the course.</param>
        /// <returns>An enumerable collection of CourseTechnology join entities, potentially including the Technology details.</returns>
        Task<IEnumerable<CourseTechnology>> GetCourseTechnologiesAsync(int courseId);

        /// <summary>
        /// Removes all technology associations for a specific course.
        /// </summary>
        /// <param name="courseId">The ID of the course.</param>
        Task ClearTechnologiesAsync(int courseId);

        
    }
}