// C:\Users\ASUS\Desktop\quizz\excellygen-lms-backend\ExcellyGenLMS.Infrastructure\Data\Repositories\Course\CourseDocumentRepository.cs
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
    public class CourseDocumentRepository : ICourseDocumentRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CourseDocumentRepository> _logger;

        public CourseDocumentRepository(ApplicationDbContext context, ILogger<CourseDocumentRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CourseDocument?> GetByIdAsync(int id)
        {
            return await _context.CourseDocuments.FindAsync(id);
        }

        public async Task<IEnumerable<CourseDocument>> GetByLessonIdAsync(int lessonId)
        {
            return await _context.CourseDocuments
                .Where(d => d.LessonId == lessonId)
                .AsNoTracking()
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<CourseDocument> AddAsync(CourseDocument document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            _context.CourseDocuments.Add(document);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Added new document '{DocumentName}' with ID {DocumentId} to lesson {LessonId}.", document.Name, document.Id, document.LessonId);
            return document;
        }

        public async Task DeleteAsync(int id)
        {
            var document = await GetByIdAsync(id);
            if (document != null)
            {
                _context.CourseDocuments.Remove(document);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted document record with ID {DocumentId} from lesson {LessonId}.", id, document.LessonId);
            }
            else
            {
                _logger.LogWarning("Attempted to delete document record with ID {DocumentId}, but it was not found.", id);
            }
        }
    }
}