using ExcellyGenLMS.Core.Entities.Course;
using ExcellyGenLMS.Core.Interfaces.Repositories.Course;
using ExcellyGenLMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Adjust namespace to match your project structure
namespace ExcellyGenLMS.Infrastructure.Data.Repositories.CourseRepo
{
    public class LessonRepository : ILessonRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LessonRepository> _logger;

        public LessonRepository(ApplicationDbContext context, ILogger<LessonRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Lesson?> GetByIdAsync(int id)
        {
            // Include documents when fetching a single lesson by ID
            return await _context.Lessons
                           .Include(l => l.Documents) // Eager load associated documents
                           .AsNoTracking() // Use if typically read-only access
                           .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<IEnumerable<Lesson>> GetByCourseIdAsync(int courseId)
        {
            return await _context.Lessons
                .Where(l => l.CourseId == courseId)
                .Include(l => l.Documents) // Include documents for lists too
                .AsNoTracking() // Use if typically read-only access
                .OrderBy(l => l.Id) // Optional: Provide default ordering
                .ToListAsync();
        }

        public async Task<Lesson> AddAsync(Lesson lesson)
        {
            if (lesson == null) throw new ArgumentNullException(nameof(lesson));

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Added new lesson '{LessonName}' with ID {LessonId} to course {CourseId}.", lesson.LessonName, lesson.Id, lesson.CourseId);
            return lesson;
        }

        public async Task UpdateAsync(Lesson lesson)
        {
            if (lesson == null) throw new ArgumentNullException(nameof(lesson));

            // Update timestamp before saving
            lesson.LastUpdatedDate = DateTime.UtcNow;

            // Mark the entity as modified. EF Core will update all properties.
            // Using Entry().State is simpler if the 'lesson' object is potentially detached
            // but represents the desired state.
            _context.Entry(lesson).State = EntityState.Modified;

            // Alternative if 'lesson' might have navigation properties that shouldn't be overwritten:
            // var existingLesson = await _context.Lessons.FindAsync(lesson.Id);
            // if (existingLesson != null) {
            //     _context.Entry(existingLesson).CurrentValues.SetValues(lesson); // Only copies scalar properties
            //     existingLesson.LastUpdatedDate = DateTime.UtcNow;
            // } else {
            //     _logger.LogError("Attempted to update Lesson with ID {LessonId}, but it was not found.", lesson.Id);
            //     throw new KeyNotFoundException($"Lesson with ID {lesson.Id} not found for update.");
            // }

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated lesson with ID {LessonId}.", lesson.Id);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating lesson {LessonId}.", lesson.Id);
                throw; // Re-throw
            }
        }

        public async Task DeleteAsync(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id); // FindAsync is efficient for PK lookups
            if (lesson != null)
            {
                _context.Lessons.Remove(lesson);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted lesson with ID {LessonId}.", id);
            }
            else
            {
                _logger.LogWarning("Attempted to delete lesson with ID {LessonId}, but it was not found.", id);
                // Optional: throw KeyNotFoundException
            }
            // Associated CourseDocuments should be handled by Cascade Delete configuration in DbContext.
        }
    }
}