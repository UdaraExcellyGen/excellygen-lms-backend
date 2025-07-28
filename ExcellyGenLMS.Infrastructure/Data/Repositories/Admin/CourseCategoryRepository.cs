using Microsoft.EntityFrameworkCore;
using ExcellyGenLMS.Core.Entities.Admin;
using ExcellyGenLMS.Core.Interfaces.Repositories.Admin;
using ExcellyGenLMS.Infrastructure.Data;

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
            var query = _context.CourseCategories
                .Include(cc => cc.Creator)
                .Include(cc => cc.Courses)
                    .ThenInclude(c => c.Enrollments)
                .AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(cc => !cc.IsDeleted);
            }

            return await query.OrderBy(cc => cc.Title).ToListAsync();
        }

        public async Task<IEnumerable<CourseCategory>> GetAllCategoryDetailsAsync(bool includeDeleted = false)
        {
            var query = _context.CourseCategories
                .Include(cc => cc.Creator)
                .Include(cc => cc.Courses.Where(c => !c.IsInactive))
                    .ThenInclude(c => c.Enrollments)
                .AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(cc => !cc.IsDeleted);
            }

            return await query.OrderBy(cc => cc.Title).ToListAsync();
        }

        public async Task<CourseCategory?> GetCategoryByIdAsync(string id)
        {
            return await _context.CourseCategories
                .Include(cc => cc.Creator)
                .Include(cc => cc.Courses)
                    .ThenInclude(c => c.Enrollments)
                .FirstOrDefaultAsync(cc => cc.Id == id);
        }

        public async Task<CourseCategory> CreateCategoryAsync(CourseCategory category)
        {
            category.CreatedAt = DateTime.UtcNow;
            category.Status = "active";

            _context.CourseCategories.Add(category);
            await _context.SaveChangesAsync();

            // Return with includes
            return await GetCategoryByIdAsync(category.Id) ?? category;
        }

        public async Task<CourseCategory> UpdateCategoryAsync(CourseCategory category)
        {
            category.UpdatedAt = DateTime.UtcNow;

            _context.CourseCategories.Update(category);
            await _context.SaveChangesAsync();

            return category;
        }

        public async Task DeleteCategoryAsync(string id)
        {
            var category = await _context.CourseCategories.FindAsync(id);
            if (category != null)
            {
                category.IsDeleted = true;
                category.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<CourseCategory?> RestoreCategoryAsync(string id)
        {
            var category = await _context.CourseCategories.FindAsync(id);
            if (category != null && category.IsDeleted)
            {
                category.IsDeleted = false;
                category.DeletedAt = null;
                category.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return await GetCategoryByIdAsync(id);
            }
            return null;
        }

        public async Task<CourseCategory> ToggleCategoryStatusAsync(string id)
        {
            var category = await _context.CourseCategories.FindAsync(id);
            if (category != null)
            {
                category.Status = category.Status == "active" ? "inactive" : "active";
                category.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            return category!;
        }

        public async Task<bool> HasActiveCoursesAsync(string categoryId)
        {
            return await _context.Set<ExcellyGenLMS.Core.Entities.Course.Course>()
                .AnyAsync(c => c.CategoryId == categoryId && !c.IsInactive);
        }

        public async Task<int> GetCoursesCountByCategoryIdAsync(string categoryId)
        {
            return await _context.Set<ExcellyGenLMS.Core.Entities.Course.Course>()
                .CountAsync(c => c.CategoryId == categoryId && !c.IsInactive);
        }

        public async Task<int> GetActiveLearnersCountByCategoryIdAsync(string categoryId)
        {
            return await _context.Set<ExcellyGenLMS.Core.Entities.Course.Course>()
                .Where(c => c.CategoryId == categoryId && !c.IsInactive)
                .SelectMany(c => c.Enrollments)
                .Select(e => e.UserId)
                .Distinct()
                .CountAsync();
        }

        public async Task<TimeSpan?> GetAverageCourseDurationByCategoryIdAsync(string categoryId)
        {
            var courses = await _context.Set<ExcellyGenLMS.Core.Entities.Course.Course>()
                .Where(c => c.CategoryId == categoryId && !c.IsInactive && c.EstimatedTime > 0)
                .ToListAsync();

            if (!courses.Any())
                return null;

            var avgHours = courses.Average(c => c.EstimatedTime);
            return TimeSpan.FromHours(avgHours);
        }
    }
}