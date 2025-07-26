using ExcellyGenLMS.Core.Entities.Course;
using ExcellyGenLMS.Core.Interfaces.Repositories.Course;
using ExcellyGenLMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Infrastructure.Data.Repositories.Course
{
    public class DocumentProgressRepository : IDocumentProgressRepository
    {
        private readonly ApplicationDbContext _context;

        public DocumentProgressRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DocumentProgress> AddAsync(DocumentProgress progress)
        {
            _context.DocumentProgress.Add(progress);
            await _context.SaveChangesAsync();
            return progress;
        }

        public async Task<DocumentProgress?> GetProgressByUserIdAndDocumentIdAsync(string userId, int documentId)
        {
            return await _context.DocumentProgress
                .FirstOrDefaultAsync(dp => dp.UserId == userId && dp.DocumentId == documentId);
        }

        public async Task<IEnumerable<DocumentProgress>> GetProgressByUserIdAndCourseIdsAsync(string userId, List<int> courseIds)
        {
            if (courseIds == null || !courseIds.Any())
            {
                return new List<DocumentProgress>();
            }

            return await _context.DocumentProgress
                .Where(dp => dp.UserId == userId && dp.Document != null && dp.Document.Lesson != null && courseIds.Contains(dp.Document.Lesson.CourseId))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task UpdateAsync(DocumentProgress progress)
        {
            _context.Entry(progress).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}