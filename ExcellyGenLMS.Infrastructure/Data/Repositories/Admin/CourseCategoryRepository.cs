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

        public async Task<List<CourseCategory>> GetAllCategoriesAsync()
        {
            return await _context.CourseCategories.ToListAsync();
        }

        public async Task<CourseCategory?> GetCategoryByIdAsync(string id)
        {
            return await _context.CourseCategories.FindAsync(id);
        }

        public async Task<CourseCategory> CreateCategoryAsync(CourseCategory category)
        {
            _context.CourseCategories.Add(category);
            await _context.SaveChangesAsync();
            return category;
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
            var category = await _context.CourseCategories.FindAsync(id)
                ?? throw new KeyNotFoundException($"Category with ID {id} not found");

            _context.CourseCategories.Remove(category);
            await _context.SaveChangesAsync();
        }

        public async Task<CourseCategory> ToggleCategoryStatusAsync(string id)
        {
            var category = await _context.CourseCategories.FindAsync(id)
                ?? throw new KeyNotFoundException($"Category with ID {id} not found");

            // Toggle the status
            category.Status = category.Status == "active" ? "inactive" : "active";
            category.UpdatedAt = DateTime.UtcNow;

            _context.CourseCategories.Update(category);
            await _context.SaveChangesAsync();

            return category;
        }

        public async Task<int> GetCoursesCountByCategoryIdAsync(string categoryId)
        {
            return await _context.Courses.CountAsync(c => c.CategoryId == categoryId);
        }
    }
}