using Microsoft.EntityFrameworkCore;
using ExcellyGenLMS.Core.Entities.Course;
using ExcellyGenLMS.Core.Interfaces.Repositories.Admin;
using ExcellyGenLMS.Infrastructure.Data;

namespace ExcellyGenLMS.Infrastructure.Data.Repositories.Admin
{
    public class CourseAdminRepository : ICourseAdminRepository
    {
        private readonly ApplicationDbContext _context;

        public CourseAdminRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Course>> GetCoursesByCategoryIdAsync(string categoryId)
        {
            return await _context.Courses
                .Include(c => c.Creator)
                .Include(c => c.Lessons)
                .Where(c => c.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<Course?> GetCourseByIdAsync(int id)
        {
            return await _context.Courses
                .Include(c => c.Creator)
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Course> UpdateCourseAsync(Course course)
        {
            course.LastUpdatedDate = DateTime.UtcNow;
            _context.Courses.Update(course);
            await _context.SaveChangesAsync();
            return course;
        }

        public async Task DeleteCourseAsync(int id)
        {
            var course = await _context.Courses.FindAsync(id)
                ?? throw new KeyNotFoundException($"Course with ID {id} not found");

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
        }
    }
}