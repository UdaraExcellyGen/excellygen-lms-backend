using ExcellyGenLMS.Core.Entities.Course;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.Course
{
    /// <summary>
    /// Interface for repository handling CourseDocument entity operations.
    /// </summary>
    public interface ICourseDocumentRepository
    {
        /// <summary>
        /// Gets a CourseDocument by its ID.
        /// </summary>
        /// <param name="id">The document ID.</param>
        /// <returns>The CourseDocument entity or null if not found.</returns>
        Task<CourseDocument?> GetByIdAsync(int id);

        /// <summary>
        /// Gets all CourseDocuments associated with a specific Lesson ID.
        /// </summary>
        /// <param name="lessonId">The ID of the parent lesson.</param>
        /// <returns>An enumerable collection of CourseDocument entities for the given lesson.</returns>
        Task<IEnumerable<CourseDocument>> GetByLessonIdAsync(int lessonId);

        /// <summary>
        /// Adds a new CourseDocument entity to the database.
        /// </summary>
        /// <param name="document">The document entity to add.</param>
        /// <returns>The added CourseDocument entity (potentially with DB-generated ID).</returns>
        Task<CourseDocument> AddAsync(CourseDocument document);

        /// <summary>
        /// Deletes a CourseDocument entity by its ID. This usually should also trigger deletion of the physical file.
        /// </summary>
        /// <param name="id">The ID of the document to delete.</param>
        Task DeleteAsync(int id);
    }
}