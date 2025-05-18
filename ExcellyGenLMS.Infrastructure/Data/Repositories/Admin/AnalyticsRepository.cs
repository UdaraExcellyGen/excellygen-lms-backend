// ExcellyGenLMS.Infrastructure/Data/Repositories/Admin/AnalyticsRepository.cs
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExcellyGenLMS.Core.Entities.Admin;
using ExcellyGenLMS.Core.Interfaces.Repositories.Admin;
using ExcellyGenLMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ExcellyGenLMS.Infrastructure.Data.Repositories.Admin
{
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<string, int>> GetEnrollmentsByCourseCategoryAsync(string categoryId)
        {
            // Get courses in this category
            var courses = await _context.Courses
                .Where(c => c.CategoryId == categoryId)
                .ToListAsync();

            var courseIds = courses.Select(c => c.Id).ToList();

            // Get enrollment counts for each course
            var enrollmentCounts = await _context.Enrollments
                .Where(e => courseIds.Contains(e.CourseId))
                .GroupBy(e => e.CourseId)
                .Select(g => new { CourseId = g.Key, Count = g.Count() })
                .ToListAsync();

            // Create a dictionary with course title and enrollment count
            var result = new Dictionary<string, int>();
            foreach (var course in courses)
            {
                var count = enrollmentCounts.FirstOrDefault(e => e.CourseId == course.Id)?.Count ?? 0;
                result.Add(course.Title, count);
            }

            return result;
        }

        public async Task<Dictionary<string, int>> GetCourseCountByCategoriesAsync()
        {
            // Get course counts by category
            var categoryCounts = await _context.Courses
                .GroupBy(c => c.CategoryId)
                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                .ToListAsync();

            // Fetch all categories
            var categories = await _context.CourseCategories.ToListAsync();

            // Create a dictionary with category name and course count
            var result = new Dictionary<string, int>();
            foreach (var category in categories)
            {
                var count = categoryCounts.FirstOrDefault(c => c.CategoryId == category.Id)?.Count ?? 0;
                result.Add(category.Title, count);
            }

            return result;
        }

        public async Task<Dictionary<string, int>> GetUserCountByRolesAsync()
        {
            // This is more complex because roles are stored as JSON
            // So we need to fetch all users and process them in memory
            var users = await _context.Users.ToListAsync();

            var roleCounts = new Dictionary<string, int>();

            // Count users by role
            foreach (var user in users)
            {
                foreach (var role in user.Roles)
                {
                    if (roleCounts.ContainsKey(role))
                    {
                        roleCounts[role]++;
                    }
                    else
                    {
                        roleCounts[role] = 1;
                    }
                }
            }

            return roleCounts;
        }

        public async Task<List<CourseCategory>> GetAllCourseCategoriesAsync()
        {
            return await _context.CourseCategories
                .Where(c => c.Status == "active")
                .ToListAsync();
        }
    }
}