using Microsoft.AspNetCore.Mvc;
using ExcellyGenLMS.Application.DTOs.Admin;
using ExcellyGenLMS.Application.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ExcellyGenLMS.Infrastructure.Data;
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
        private readonly ILogger<CourseCategoriesController> _logger;
        private readonly ApplicationDbContext _context;

        public CourseCategoriesController(
            ICourseCategoryService categoryService,
            ILogger<CourseCategoriesController> logger,
            ApplicationDbContext context)
        {
            _categoryService = categoryService;
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Learner")]
        public async Task<ActionResult<List<CourseCategoryDto>>> GetAllCategories([FromQuery] bool includeDeleted = false)
        {
            try
            {
                // FIXED: Check current active role, not just available roles
                var currentRole = User.FindFirst("CurrentRole")?.Value ?? "";
                var isCurrentlyAdmin = currentRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);

                _logger.LogInformation("Categories request - Active Role: {CurrentRole}, IsAdmin: {IsAdmin}", currentRole, isCurrentlyAdmin);

                // Get categories from service
                var allCategories = await _categoryService.GetAllCategoriesAsync(isCurrentlyAdmin && includeDeleted);

                if (isCurrentlyAdmin)
                {
                    _logger.LogInformation("ADMIN: Returning {Count} categories", allCategories.Count);
                    return Ok(allCategories);
                }

                // LEARNER FILTERING - Only active, non-deleted categories with published courses
                var learnerCategories = allCategories
                    .Where(c => !c.IsDeleted &&
                               string.Equals(c.Status, "active", StringComparison.OrdinalIgnoreCase) &&
                               c.TotalCourses > 0) // Only show categories that have courses
                    .ToList();

                _logger.LogInformation("LEARNER: Filtered to {FilteredCount} active categories with courses from {TotalCount}",
                    learnerCategories.Count, allCategories.Count);

                return Ok(learnerCategories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                return StatusCode(500, new { message = "Failed to retrieve categories. Please try again." });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Learner")]
        public async Task<ActionResult<CourseCategoryDto>> GetCategoryById(string id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                var currentRole = User.FindFirst("CurrentRole")?.Value ?? "";
                var isCurrentlyAdmin = currentRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);

                // TEST CASE A-03 & L-01: Non-admin users cannot access inactive/deleted categories
                if (!isCurrentlyAdmin && (category.IsDeleted || !category.Status.Equals("active", StringComparison.OrdinalIgnoreCase)))
                {
                    _logger.LogWarning("Non-admin user attempted to access inactive/deleted category: {CategoryId}", id);
                    return NotFound(new { message = "Course category not found or is no longer available." });
                }

                return Ok(category);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Category with ID {id} not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category: {CategoryId}", id);
                return StatusCode(500, new { message = "An error occurred." });
            }
        }

        [HttpGet("{id}/courses")]
        [Authorize(Roles = "Admin,CourseCoordinator")]
        public async Task<ActionResult> GetCoursesByCategory(string id)
        {
            try
            {
                var currentRole = User.FindFirst("CurrentRole")?.Value ?? "";
                _logger.LogInformation($"Getting courses for category: {id} by role: {currentRole}");

                // First verify the category exists
                var category = await _categoryService.GetCategoryByIdAsync(id);

                // Get courses directly from database using Entity Framework
                var courses = await _context.Courses
                    .Include(c => c.Creator)
                    .Include(c => c.Lessons)
                    .Where(c => c.CategoryId == id && !c.IsInactive)
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new
                    {
                        id = c.Id,
                        title = c.Title,
                        description = c.Description,
                        status = c.Status.ToString(),
                        createdAt = c.CreatedAt,
                        createdAtFormatted = c.CreatedAt.ToString("yyyy-MM-dd"),
                        creatorId = c.CreatorId,
                        categoryId = c.CategoryId,
                        estimatedTime = c.EstimatedTime,
                        coursePoints = c.CoursePoints,
                        thumbnailImagePath = c.ThumbnailImagePath,
                        creator = c.Creator != null ? new
                        {
                            id = c.Creator.Id,
                            name = c.Creator.Name,
                            email = c.Creator.Email
                        } : null,
                        lessons = c.Lessons.Select(l => new
                        {
                            id = l.Id,
                            lessonName = l.LessonName
                        }).ToList()
                    })
                    .ToListAsync();

                _logger.LogInformation($"Retrieved {courses.Count} courses for category: {id}");

                return Ok(courses);
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning($"Category not found: {id}");
                return NotFound(new { message = $"Category with ID {id} not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting courses for category: {id}");
                return StatusCode(500, new { message = "An error occurred while retrieving courses." });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CourseCategoryDto>> CreateCategory([FromBody] CreateCourseCategoryDto dto)
        {
            try
            {
                var category = await _categoryService.CreateCategoryAsync(dto);
                _logger.LogInformation("Created new category: {CategoryId} - {Title}", category.Id, category.Title);
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
        public async Task<ActionResult<CourseCategoryDto>> UpdateCategory(string id, [FromBody] UpdateCourseCategoryDto dto)
        {
            try
            {
                var updatedCategory = await _categoryService.UpdateCategoryAsync(id, dto);
                _logger.LogInformation("Updated category: {CategoryId} - Status: {Status}", id, dto.Status);
                return Ok(updatedCategory);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Category with ID {id} not found." });
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
                await _categoryService.DeleteCategoryAsync(id);
                _logger.LogInformation("Successfully soft-deleted category: {CategoryId}", id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Category with ID {id} not found." });
            }
            catch (InvalidOperationException ex)
            {
                // TEST CASE A-05: Proper error message for categories with active courses
                _logger.LogWarning("Cannot delete category {CategoryId}: {Reason}", id, ex.Message);
                return BadRequest(new { message = $"Cannot delete category. {ex.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category: {CategoryId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the category." });
            }
        }

        [HttpPost("{id}/restore")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CourseCategoryDto>> RestoreCategory(string id)
        {
            try
            {
                var restoredCategory = await _categoryService.RestoreCategoryAsync(id);
                _logger.LogInformation("Successfully restored category: {CategoryId} - {Title}", id, restoredCategory.Title);
                return Ok(restoredCategory);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Category with ID {id} not found in trash." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring category: {CategoryId}", id);
                return StatusCode(500, new { message = "An error occurred while restoring the category." });
            }
        }

        [HttpPatch("{id}/toggle-status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CourseCategoryDto>> ToggleCategoryStatus(string id)
        {
            try
            {
                var updatedCategory = await _categoryService.ToggleCategoryStatusAsync(id);
                var action = updatedCategory.Status == "active" ? "activated" : "deactivated";
                _logger.LogInformation("Successfully {Action} category: {CategoryId} - {Title}", action, id, updatedCategory.Title);
                return Ok(updatedCategory);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Category with ID {id} not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling status for category: {CategoryId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the category status." });
            }
        }
    }
}