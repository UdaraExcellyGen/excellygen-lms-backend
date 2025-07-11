using Microsoft.AspNetCore.Mvc;
using ExcellyGenLMS.Application.DTOs.Admin;
using ExcellyGenLMS.Application.DTOs.Course;
using ExcellyGenLMS.Application.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using ExcellyGenLMS.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExcellyGenLMS.API.Controllers.Admin
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourseCategoriesController : ControllerBase
    {
        private readonly ICourseCategoryService _categoryService;
        private readonly ICourseAdminService _courseService;
        private readonly ILogger<CourseCategoriesController> _logger;

        public CourseCategoriesController(
            ICourseCategoryService categoryService,
            ICourseAdminService courseService,
            ILogger<CourseCategoriesController> logger)
        {
            _categoryService = categoryService;
            _courseService = courseService;
            _logger = logger;
        }

        // READ OPERATIONS - Allow both Admin and Learner with optimization
        [HttpGet]
        [Authorize(Roles = "Admin,Learner")]
        public async Task<ActionResult<List<CourseCategoryDto>>> GetAllCategories()
        {
            try
            {
                var userRole = User.IsInRole("Admin") ? "Admin" : "Learner";
                _logger.LogInformation("Getting all course categories for role: {Role}", userRole);

                var categories = await _categoryService.GetAllCategoriesAsync();

                // If user is Learner, only return active categories
                if (User.IsInRole("Learner") && !User.IsInRole("Admin"))
                {
                    categories = categories.Where(c => string.Equals(c.Status, "active", StringComparison.OrdinalIgnoreCase)).ToList();
                }

                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all categories");
                return StatusCode(500, new { message = "An error occurred while retrieving categories." });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Learner")]
        public async Task<ActionResult<CourseCategoryDto>> GetCategoryById(string id)
        {
            try
            {
                var userRole = User.IsInRole("Admin") ? "Admin" : "Learner";
                _logger.LogInformation("Getting course category with ID: {CategoryId} for role: {Role}", id, userRole);

                var category = await _categoryService.GetCategoryByIdAsync(id);

                // If user is Learner, only return if category is active
                if (User.IsInRole("Learner") && !User.IsInRole("Admin"))
                {
                    if (!string.Equals(category.Status, "active", StringComparison.OrdinalIgnoreCase))
                    {
                        return NotFound(new { message = "Course category not found." });
                    }
                }

                return Ok(category);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Category not found: {CategoryId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category: {CategoryId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the category." });
            }
        }

        // OPTIMIZATION: Combined endpoint to get courses and stats in one call
        [HttpGet("{categoryId}/courses-with-stats")]
        [Authorize(Roles = "Admin,Learner")]
        public async Task<ActionResult<object>> GetCoursesWithStatsByCategory(string categoryId)
        {
            try
            {
                var userRole = User.IsInRole("Admin") ? "Admin" : "Learner";
                _logger.LogInformation("Getting courses and stats for category with ID: {CategoryId}, role: {Role}", categoryId, userRole);

                // First verify the category exists and is accessible
                var category = await _categoryService.GetCategoryByIdAsync(categoryId);

                // If user is Learner, ensure category is active
                if (User.IsInRole("Learner") && !User.IsInRole("Admin"))
                {
                    if (!string.Equals(category.Status, "active", StringComparison.OrdinalIgnoreCase))
                    {
                        return NotFound(new { message = "Course category not found." });
                    }
                }

                // OPTIMIZATION: Get courses and stats in parallel
                var coursesTask = _courseService.GetCoursesByCategoryIdAsync(categoryId);

                // Only get stats for Admin users to reduce unnecessary queries
                Task<ExcellyGenLMS.Application.Services.Admin.AdminCategoryStatsDto> statsTask = null;
                if (User.IsInRole("Admin"))
                {
                    statsTask = _courseService.GetCategoryStatsAsync(categoryId);
                }

                var courses = await coursesTask;

                // If user is Learner, only return published courses
                if (User.IsInRole("Learner") && !User.IsInRole("Admin"))
                {
                    courses = courses.Where(c => c.Status.ToString().Equals("Published", StringComparison.OrdinalIgnoreCase)).ToList();
                }

                var result = new
                {
                    Category = category,
                    Courses = courses,
                    Stats = statsTask != null ? await statsTask : null,
                    TotalCourses = courses.Count
                };

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Category not found: {CategoryId}", categoryId);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting courses and stats for category: {CategoryId}", categoryId);
                return StatusCode(500, new { message = "An error occurred while retrieving courses and stats." });
            }
        }

        // OPTIMIZATION: Keep original endpoint for backward compatibility
        [HttpGet("{categoryId}/courses")]
        [Authorize(Roles = "Admin,Learner")]
        public async Task<ActionResult<List<CourseDto>>> GetCoursesByCategory(string categoryId)
        {
            try
            {
                var userRole = User.IsInRole("Admin") ? "Admin" : "Learner";
                _logger.LogInformation("Getting courses for category with ID: {CategoryId}, role: {Role}", categoryId, userRole);

                // First verify the category exists and is accessible
                var category = await _categoryService.GetCategoryByIdAsync(categoryId);

                // If user is Learner, ensure category is active
                if (User.IsInRole("Learner") && !User.IsInRole("Admin"))
                {
                    if (!string.Equals(category.Status, "active", StringComparison.OrdinalIgnoreCase))
                    {
                        return NotFound(new { message = "Course category not found." });
                    }
                }

                // Get courses for this category
                var courses = await _courseService.GetCoursesByCategoryIdAsync(categoryId);

                // If user is Learner, only return published courses
                if (User.IsInRole("Learner") && !User.IsInRole("Admin"))
                {
                    courses = courses.Where(c => c.Status.ToString().Equals("Published", StringComparison.OrdinalIgnoreCase)).ToList();
                }

                return Ok(courses);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Category not found: {CategoryId}", categoryId);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting courses for category: {CategoryId}", categoryId);
                return StatusCode(500, new { message = "An error occurred while retrieving courses." });
            }
        }

        // WRITE OPERATIONS - Admin only
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CourseCategoryDto>> CreateCategory([FromBody] CreateCourseCategoryDto createCategoryDto)
        {
            try
            {
                _logger.LogInformation("Creating new course category");
                var category = await _categoryService.CreateCategoryAsync(createCategoryDto);

                return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return StatusCode(500, new { message = "An error occurred while creating the category." });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CourseCategoryDto>> UpdateCategory(string id, [FromBody] UpdateCourseCategoryDto updateCategoryDto)
        {
            try
            {
                _logger.LogInformation("Updating course category with ID: {CategoryId}", id);
                var category = await _categoryService.UpdateCategoryAsync(id, updateCategoryDto);

                return Ok(category);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Category not found: {CategoryId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category: {CategoryId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the category." });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteCategory(string id)
        {
            try
            {
                _logger.LogInformation("Deleting course category with ID: {CategoryId}", id);
                await _categoryService.DeleteCategoryAsync(id);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Category not found: {CategoryId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category: {CategoryId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the category." });
            }
        }

        [HttpPatch("{id}/toggle-status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CourseCategoryDto>> ToggleCategoryStatus(string id)
        {
            try
            {
                _logger.LogInformation("Toggling status for course category with ID: {CategoryId}", id);
                var category = await _categoryService.ToggleCategoryStatusAsync(id);

                return Ok(category);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Category not found: {CategoryId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling category status: {CategoryId}", id);
                return StatusCode(500, new { message = "An error occurred while toggling the category status." });
            }
        }
    }
}