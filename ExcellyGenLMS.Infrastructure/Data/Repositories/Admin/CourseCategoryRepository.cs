using ExcellyGenLMS.Core.Entities.Admin;
using ExcellyGenLMS.Core.Interfaces.Repositories.Admin;
using ExcellyGenLMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Infrastructure.Data.Repositories.Admin
{
    public class CourseCategoryRepository : ICourseCategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CourseCategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CourseCategory>> GetAllCategoriesAsync(bool includeDeleted = false)
        {
            var query = _context.CourseCategories.AsQueryable();
            if (!includeDeleted)
            {
                query = query.Where(c => !c.IsDeleted);
            }
            return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        }

        public async Task<IEnumerable<CourseCategory>> GetAllCategoryDetailsAsync(bool includeDeleted = false)
        {
            var query = _context.CourseCategories
                                .Include(c => c.Courses)
                                .ThenInclude(course => course.Enrollments)
                                .AsSplitQuery() // Optimization to prevent cartesian explosion
                                .AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(c => !c.IsDeleted);
            }

            return await query.ToListAsync();
        }

        public async Task<CourseCategory?> GetCategoryByIdAsync(string id) => await _context.CourseCategories.FindAsync(id);
        public async Task<CourseCategory> CreateCategoryAsync(CourseCategory category) { _context.CourseCategories.Add(category); await _context.SaveChangesAsync(); return category; }
        public async Task<CourseCategory> UpdateCategoryAsync(CourseCategory category) { category.UpdatedAt = DateTime.UtcNow; _context.CourseCategories.Update(category); await _context.SaveChangesAsync(); return category; }
        public async Task DeleteCategoryAsync(string id) { var cat = await _context.CourseCategories.FindAsync(id); if (cat != null) { cat.IsDeleted = true; cat.DeletedAt = DateTime.UtcNow; await _context.SaveChangesAsync(); } }
        public async Task<CourseCategory?> RestoreCategoryAsync(string id) { var cat = await _context.CourseCategories.FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted); if (cat != null) { cat.IsDeleted = false; cat.DeletedAt = null; await _context.SaveChangesAsync(); } return cat; }
        public async Task<bool> HasActiveCoursesAsync(string categoryId) => await _context.Courses.AnyAsync(c => c.CategoryId == categoryId);
        public async Task<CourseCategory> ToggleCategoryStatusAsync(string id) { var cat = await _context.CourseCategories.FindAsync(id) ?? throw new KeyNotFoundException(); cat.Status = cat.Status == "active" ? "inactive" : "active"; await _context.SaveChangesAsync(); return cat; }
        public async Task<int> GetCoursesCountByCategoryIdAsync(string categoryId) => await _context.Courses.CountAsync(c => c.CategoryId == categoryId && !c.IsInactive);

        public async Task<int> GetActiveLearnersCountByCategoryIdAsync(string categoryId)
        {
            return await _context.Enrollments
                                 .Where(e => e.Course != null && e.Course.CategoryId == categoryId)
                                 .Select(e => e.UserId)
                                 .Distinct()
                                 .CountAsync();
        }

        public async Task<TimeSpan?> GetAverageCourseDurationByCategoryIdAsync(string categoryId)
        {
            var courses = await _context.Courses.Where(c => c.CategoryId == categoryId && !c.IsInactive).ToListAsync();
            if (!courses.Any() || courses.All(c => c.EstimatedTime == 0)) return null;
            var avg = courses.Average(c => (double?)c.EstimatedTime);
            return avg.HasValue ? TimeSpan.FromHours(avg.Value) : null;
        }
    }
}