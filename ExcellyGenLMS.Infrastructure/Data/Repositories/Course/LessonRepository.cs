// C:\Users\ASUS\Desktop\quizz\excellygen-lms-backend\ExcellyGenLMS.Infrastructure\Data\Repositories\Course\LessonRepository.cs
using ExcellyGenLMS.Core.Entities.Course;
using ExcellyGenLMS.Core.Interfaces.Repositories.Course;
using ExcellyGenLMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Namespace: ExcellyGenLMS.Infrastructure.Data.Repositories.Course
namespace ExcellyGenLMS.Infrastructure.Data.Repositories.Course
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
            return await _context.Lessons
                           .Include(l => l.Documents)
                           .AsNoTracking()
                           .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<IEnumerable<Lesson>> GetByCourseIdAsync(int courseId)
        {
            return await _context.Lessons
                .Where(l => l.CourseId == courseId)
                .Include(l => l.Documents)
                .AsNoTracking()
                .OrderBy(l => l.Id)
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

            lesson.LastUpdatedDate = DateTime.UtcNow;

            _context.Entry(lesson).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated lesson with ID {LessonId}.", lesson.Id);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating lesson {LessonId}.", lesson.Id);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson != null)
            {
                _context.Lessons.Remove(lesson);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted lesson with ID {LessonId}.", id);
            }
            else
            {
                _logger.LogWarning("Attempted to delete lesson with ID {LessonId}, but it was not found.", id);
            }
        }
    }
}