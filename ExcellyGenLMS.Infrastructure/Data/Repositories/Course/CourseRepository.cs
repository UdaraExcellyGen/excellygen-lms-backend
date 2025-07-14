using ExcellyGenLMS.Core.Entities.Course;
using ExcellyGenLMS.Core.Interfaces.Repositories.Course;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExcellyGenLMS.Core.Enums;

namespace ExcellyGenLMS.Infrastructure.Data.Repositories.Course
{
    public class CourseRepository : ICourseRepository
    {
        private readonly ApplicationDbContext _context;

        public CourseRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Core.Entities.Course.Course?> GetByIdAsync(int id)
        {
            return await _context.Courses.FindAsync(id);
        }

        public async Task<Core.Entities.Course.Course?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Creator)
                .Include(c => c.CourseTechnologies).ThenInclude(ct => ct.Technology)
                .Include(c => c.Lessons).ThenInclude(l => l.Documents)
                .AsSplitQuery()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Core.Entities.Course.Course>> GetAllAsync()
        {
            return await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Creator)
                .Include(c => c.CourseTechnologies).ThenInclude(ct => ct.Technology)
                .AsNoTracking()
                .OrderBy(c => c.Title)
                .ToListAsync();
        }

        public async Task<IEnumerable<Core.Entities.Course.Course>> GetAllPublishedCoursesWithDetailsAsync()
        {
            return await _context.Courses
                .Where(c => c.Status == CourseStatus.Published)
                .Include(c => c.Category)
                .Include(c => c.Creator)
                .Include(c => c.CourseTechnologies).ThenInclude(ct => ct.Technology)
                .Include(c => c.Lessons).ThenInclude(l => l.Documents)
                .AsSplitQuery()
                .AsNoTracking()
                .OrderBy(c => c.Title)
                .ToListAsync();
        }

        public async Task<IEnumerable<Lesson>> GetLessonsByCourseIdAsync(int courseId)
        {
            return await _context.Lessons
                .Where(l => l.CourseId == courseId)
                .Include(l => l.Documents)
                .AsNoTracking()
                .OrderBy(l => l.Id)
                .ToListAsync();
        }

        public async Task<IEnumerable<Lesson>> GetLessonsByCourseIdsAsync(List<int> courseIds)
        {
            return await _context.Lessons
                .Where(l => courseIds.Contains(l.CourseId))
                .Include(l => l.Documents)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Lesson?> GetLessonWithDocumentsAsync(int lessonId)
        {
            return await _context.Lessons
                .Include(l => l.Documents)
                .FirstOrDefaultAsync(l => l.Id == lessonId);
        }

        public async Task<Core.Entities.Course.Course> AddAsync(Core.Entities.Course.Course course)
        {
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            return course;
        }

        public async Task UpdateAsync(Core.Entities.Course.Course course)
        {
            _context.Entry(course).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddTechnologyAsync(int courseId, string technologyId)
        {
            var courseTechnology = new CourseTechnology { CourseId = courseId, TechnologyId = technologyId };
            _context.CourseTechnologies.Add(courseTechnology);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveTechnologyAsync(int courseId, string technologyId)
        {
            var courseTechnology = await _context.CourseTechnologies
                .FirstOrDefaultAsync(ct => ct.CourseId == courseId && ct.TechnologyId == technologyId);
            if (courseTechnology != null)
            {
                _context.CourseTechnologies.Remove(courseTechnology);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<CourseTechnology>> GetCourseTechnologiesAsync(int courseId)
        {
            return await _context.CourseTechnologies
                .Where(ct => ct.CourseId == courseId)
                .Include(ct => ct.Technology)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task ClearTechnologiesAsync(int courseId)
        {
            var existingTechnologies = await _context.CourseTechnologies
                .Where(ct => ct.CourseId == courseId)
                .ToListAsync();
            _context.CourseTechnologies.RemoveRange(existingTechnologies);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetTotalPublishedCoursesCountAsync()
        {
            return await _context.Courses
                .AsNoTracking()
                .CountAsync(c => c.Status == Core.Enums.CourseStatus.Published);
        }

        public async Task<TimeSpan?> GetOverallAverageCourseDurationAsync()
        {
            var averageHours = await _context.Courses
                .AsNoTracking()
                .Where(c => c.Status == Core.Enums.CourseStatus.Published)
                .Select(c => (double?)c.EstimatedTime)
                .AverageAsync();
            return averageHours.HasValue ? TimeSpan.FromHours(averageHours.Value) : (TimeSpan?)null;
        }
    }
}