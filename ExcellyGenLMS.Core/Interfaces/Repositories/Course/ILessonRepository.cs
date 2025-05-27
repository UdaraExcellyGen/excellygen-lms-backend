using ExcellyGenLMS.Core.Entities.Course;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.Course
{
    /// <summary>
    /// Interface for repository handling Lesson entity operations.
    /// </summary>
    public interface ILessonRepository
    {
        /// <summary>
        /// Gets a Lesson by its ID, potentially including its documents.
        /// </summary>
        /// <param name="id">The lesson ID.</param>
        /// <returns>The Lesson entity or null if not found.</returns>
        Task<Lesson?> GetByIdAsync(int id);

        /// <summary>
        /// Gets all Lessons associated with a specific Course ID.
        /// </summary>
        /// <param name="courseId">The ID of the parent course.</param>
        /// <returns>An enumerable collection of Lesson entities for the given course.</returns>
        Task<IEnumerable<Lesson>> GetByCourseIdAsync(int courseId);

        /// <summary>
        /// Adds a new Lesson entity to the database.
        /// </summary>
        /// <param name="lesson">The lesson entity to add.</param>
        /// <returns>The added Lesson entity (potentially with DB-generated ID).</returns>
        Task<Lesson> AddAsync(Lesson lesson);

        /// <summary>
        /// Updates an existing Lesson entity in the database.
        /// </summary>
        /// <param name="lesson">The lesson entity with updated values.</param>
        Task UpdateAsync(Lesson lesson);

        /// <summary>
        /// Deletes a Lesson entity by its ID. Associated documents might be cascade deleted based on DB setup.
        /// </summary>
        /// <param name="id">The ID of the lesson to delete.</param>
        Task DeleteAsync(int id);
    }
}