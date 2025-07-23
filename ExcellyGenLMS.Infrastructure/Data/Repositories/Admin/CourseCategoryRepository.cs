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

        public async Task<CourseCategory?> GetCategoryByIdAsync(string id) => await _context.CourseCategories.FindAsync(id);

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
            var category = await _context.CourseCategories.FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted);
            if (category != null)
            {
                category.IsDeleted = false;
                category.DeletedAt = null;
                await _context.SaveChangesAsync();
            }
            return category;
        }

        // FIXED: Simplified and optimized - only check for any enrollments, not complex queries
        public async Task<bool> HasActiveCoursesAsync(string categoryId)
        {
            // Simple check: if there are ANY enrollments in courses within this category
            return await _context.Enrollments
                .AnyAsync(e => e.Course != null && e.Course.CategoryId == categoryId);
        }

        public async Task<CourseCategory> ToggleCategoryStatusAsync(string id)
        {
            var category = await _context.CourseCategories.FindAsync(id) ?? throw new KeyNotFoundException($"Category with ID {id} not found");
            category.Status = (category.Status == "active") ? "inactive" : "active";
            category.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return category;
        }

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
            var courses = await _context.Courses
                                       .Where(c => c.CategoryId == categoryId && !c.IsInactive)
                                       .ToListAsync();
            if (!courses.Any() || courses.All(c => c.EstimatedTime == 0)) return null;
            var averageHours = courses.Average(c => (double?)c.EstimatedTime);
            return averageHours.HasValue ? TimeSpan.FromHours(averageHours.Value) : null;
        }

        // NEW METHOD: Check if user has active enrollments in this category
        public async Task<bool> HasActiveEnrollmentsAsync(string categoryId, string userId)
        {
            return await _context.Enrollments
                .AnyAsync(e => e.UserId == userId &&
                              e.Course != null &&
                              e.Course.CategoryId == categoryId &&
                              !e.Course.IsInactive);
        }

        // NEW METHOD: Get categories where user has enrollments (for learner filtering)
        public async Task<List<string>> GetCategoryIdsWithUserEnrollmentsAsync(string userId)
        {
            return await _context.Enrollments
                .Where(e => e.UserId == userId &&
                           e.Course != null &&
                           !e.Course.IsInactive &&
                           e.Course.CategoryId != null) // Added null check
                .Select(e => e.Course.CategoryId!)  // Added null-forgiving operator
                .Distinct()
                .ToListAsync();
        }
    }
}