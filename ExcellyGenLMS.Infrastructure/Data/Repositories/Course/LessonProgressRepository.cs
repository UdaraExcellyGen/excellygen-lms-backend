using ExcellyGenLMS.Core.Entities.Course;
using ExcellyGenLMS.Core.Interfaces.Repositories.Course;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Infrastructure.Data.Repositories.Course
{
    public class LessonProgressRepository : ILessonProgressRepository
    {
        private readonly ApplicationDbContext _context;

        public LessonProgressRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<LessonProgress?> GetByIdAsync(int id)
        {
            return await _context.LessonProgress
                                 .Include(lp => lp.User)
                                 .Include(lp => lp.Lesson)
                                 .FirstOrDefaultAsync(lp => lp.Id == id);
        }

        public async Task<LessonProgress?> GetProgressByUserIdAndLessonIdAsync(string userId, int lessonId)
        {
            return await _context.LessonProgress
                                 .FirstOrDefaultAsync(lp => lp.UserId == userId && lp.LessonId == lessonId);
        }

        public async Task<IEnumerable<LessonProgress>> GetProgressByUserIdAndCourseIdAsync(string userId, int courseId)
        {
            return await _context.LessonProgress
                                 .Where(lp => lp.UserId == userId && lp.Lesson != null && lp.Lesson.CourseId == courseId)
                                 .Include(lp => lp.Lesson)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<LessonProgress>> GetProgressByUserIdAndCourseIdsAsync(string userId, List<int> courseIds)
        {
            return await _context.LessonProgress
                .Where(lp => lp.UserId == userId && lp.Lesson != null && courseIds.Contains(lp.Lesson.CourseId))
                .Include(lp => lp.Lesson)
                .ToListAsync();
        }

        public async Task<LessonProgress> AddAsync(LessonProgress progress)
        {
            _context.LessonProgress.Add(progress);
            await _context.SaveChangesAsync();
            return progress;
        }

        public async Task UpdateAsync(LessonProgress progress)
        {
            _context.Entry(progress).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var progress = await _context.LessonProgress.FindAsync(id);
            if (progress != null)
            {
                _context.LessonProgress.Remove(progress);
                await _context.SaveChangesAsync();
            }
        }
    }
}