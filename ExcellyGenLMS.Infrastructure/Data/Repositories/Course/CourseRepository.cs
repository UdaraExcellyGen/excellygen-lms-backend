using ExcellyGenLMS.Core.Entities.Course;
using ExcellyGenLMS.Core.Interfaces.Repositories.Course;
using ExcellyGenLMS.Infrastructure.Data; // Access to DbContext
using Microsoft.EntityFrameworkCore;      // For EF Core extensions like Include, FirstOrDefaultAsync, etc.
using Microsoft.Extensions.Logging;     // For logging
using System;                         // For ArgumentNullException
using System.Collections.Generic;       // For IEnumerable, List
using System.Linq;                      // For LINQ methods
using System.Threading.Tasks;           // For Task, async/await

// Adjust namespace to match your project structure
namespace ExcellyGenLMS.Infrastructure.Data.Repositories.CourseRepo
{
    public class CourseRepository : ICourseRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CourseRepository> _logger;

        public CourseRepository(ApplicationDbContext context, ILogger<CourseRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Core.Entities.Course.Course?> GetByIdAsync(int id)
        {
            return await _context.Courses.FindAsync(id);
        }

        public async Task<Core.Entities.Course.Course?> GetByIdWithDetailsAsync(int id)
        {
            // Include all necessary related data for displaying detailed course info
            return await _context.Courses
                .Include(c => c.Category)               // Include the course category
                .Include(c => c.Creator)                // Include the user who created it
                .Include(c => c.Lessons)!               // Include the list of lessons
                    .ThenInclude(l => l.Documents)      // For each lesson, include its documents
                .Include(c => c.CourseTechnologies)!    // Include the join table entries
                    .ThenInclude(ct => ct.Technology)   // For each join table entry, include the actual Technology details
                .AsNoTracking() // Use AsNoTracking for read-only operations to improve performance if changes won't be saved back immediately through this entity instance. Remove if you intend to modify the retrieved entity and save.
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Core.Entities.Course.Course>> GetAllAsync()
        {
            // Basic fetch. In production, add includes needed for list views (often fewer than GetByIdWithDetailsAsync),
            // filtering, pagination, and sorting.
            return await _context.Courses
               .Include(c => c.Category) // Example: Include category for display
               .Include(c => c.Creator) // Example: Include creator name for display
               .AsNoTracking() // Good practice for lists unless editing directly
               .ToListAsync();
        }

        public async Task<Core.Entities.Course.Course> AddAsync(Core.Entities.Course.Course course)
        {
            if (course == null) throw new ArgumentNullException(nameof(course));

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Added new course with ID {CourseId}.", course.Id);
            return course; // Return the entity with potentially DB-generated ID
        }

        public async Task UpdateAsync(Core.Entities.Course.Course course)
        {
            if (course == null) throw new ArgumentNullException(nameof(course));

            // Check if the entity is already tracked. If not, attach and mark as modified.
            // If it is tracked, EF Core will automatically detect changes on SaveChangesAsync.
            var existingEntry = _context.Entry(course);

            if (existingEntry.State == EntityState.Detached)
            {
                // If the passed object isn't tracked, find the existing one and update its values.
                // This prevents accidentally tracking two instances of the same entity.
                var existingCourse = await _context.Courses.FindAsync(course.Id);
                if (existingCourse != null)
                {
                    // Copy values from the incoming 'course' to the tracked 'existingCourse'
                    _context.Entry(existingCourse).CurrentValues.SetValues(course);
                    _logger.LogDebug("Updating existing tracked course entity {CourseId}", course.Id);
                }
                else
                {
                    // Should not happen if update logic is correct (entity should exist)
                    _logger.LogError("Attempted to update course with ID {CourseId}, but it was not found in the database.", course.Id);
                    throw new KeyNotFoundException($"Course with ID {course.Id} not found for update.");
                }
            }
            else
            {
                // If it's already tracked, just ensure its state is Modified (though EF might do this automatically).
                existingEntry.State = EntityState.Modified;
                _logger.LogDebug("Updating detached/modified course entity {CourseId}", course.Id);
            }

            // Update the timestamp before saving
            course.LastUpdatedDate = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated course with ID {CourseId}.", course.Id);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Handle concurrency issues if necessary (e.g., reload entity, notify user)
                _logger.LogError(ex, "Concurrency error updating course {CourseId}.", course.Id);
                throw; // Re-throw for higher levels to handle
            }
        }


        public async Task DeleteAsync(int id)
        {
            var course = await GetByIdAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted course with ID {CourseId}.", id);
            }
            else
            {
                _logger.LogWarning("Attempted to delete course with ID {CourseId}, but it was not found.", id);
                // Optional: Throw KeyNotFoundException if caller should know it didn't exist.
            }
            // Associated Lessons/Documents/CourseTechnologies should be handled by Cascade Delete configuration in DbContext.
        }

        public async Task AddTechnologyAsync(int courseId, string technologyId)
        {
            var courseTechnology = new CourseTechnology { CourseId = courseId, TechnologyId = technologyId };

            // Check if the association already exists to prevent duplicate entries or exceptions
            var exists = await _context.CourseTechnologies
                .AnyAsync(ct => ct.CourseId == courseId && ct.TechnologyId == technologyId);

            if (!exists)
            {
                _context.CourseTechnologies.Add(courseTechnology);
                await _context.SaveChangesAsync();
                _logger.LogDebug("Added technology link: Course {CourseId} <-> Tech {TechId}", courseId, technologyId);
            }
            else
            {
                _logger.LogDebug("Technology link already exists: Course {CourseId} <-> Tech {TechId}", courseId, technologyId);
            }
        }

        public async Task RemoveTechnologyAsync(int courseId, string technologyId)
        {
            var courseTechnology = await _context.CourseTechnologies
                .FirstOrDefaultAsync(ct => ct.CourseId == courseId && ct.TechnologyId == technologyId);

            if (courseTechnology != null)
            {
                _context.CourseTechnologies.Remove(courseTechnology);
                await _context.SaveChangesAsync();
                _logger.LogDebug("Removed technology link: Course {CourseId} <-> Tech {TechId}", courseId, technologyId);
            }
            else
            {
                _logger.LogWarning("Attempted to remove non-existent technology link: Course {CourseId} <-> Tech {TechId}", courseId, technologyId);
            }
        }

        public async Task<IEnumerable<CourseTechnology>> GetCourseTechnologiesAsync(int courseId)
        {
            return await _context.CourseTechnologies
                .Where(ct => ct.CourseId == courseId)
                .Include(ct => ct.Technology) // Include the related Technology details
                .AsNoTracking() // Good for read-only list
                .ToListAsync();
        }



        public async Task ClearTechnologiesAsync(int courseId)
        {
            // Find all existing technology associations for the course
            var technologies = await _context.CourseTechnologies
                .Where(ct => ct.CourseId == courseId)
                .ToListAsync();

            if (technologies.Any())
            {
                _context.CourseTechnologies.RemoveRange(technologies);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Cleared {Count} technology links for course {CourseId}.", technologies.Count, courseId);
            }
            else
            {
                _logger.LogInformation("No technology links to clear for course {CourseId}.", courseId);
            }
        }
    }
}