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
using System.Security.Claims;
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
                var currentRole = User.FindFirst("CurrentRole")?.Value ?? "";
                var isCurrentlyAdmin = currentRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);

                _logger.LogInformation("Categories request - Active Role: {CurrentRole}, IsAdmin: {IsAdmin}", currentRole, isCurrentlyAdmin);

                if (isCurrentlyAdmin)
                {
                    var allCategories = await _categoryService.GetAllCategoriesAsync(includeDeleted);
                    _logger.LogInformation("ADMIN: Returning {Count} categories", allCategories.Count);
                    return Ok(allCategories);
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Could not determine user ID for learner categories request. Available claims: {Claims}",
                        string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}")));
                    return BadRequest(new { message = "User identification failed" });
                }

                var learnerCategories = await _categoryService.GetLearnerAccessibleCategoriesAsync(userId);

                _logger.LogInformation("LEARNER: Returning {Count} accessible categories for user {UserId}",
                    learnerCategories.Count, userId);

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

                if (isCurrentlyAdmin)
                {
                    return Ok(category);
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Could not determine user ID for category access check: {CategoryId}", id);
                    return BadRequest(new { message = "User identification failed" });
                }

                bool canAccess = !category.IsDeleted &&
                               (category.Status.Equals("active", StringComparison.OrdinalIgnoreCase) ||
                                await _categoryService.HasUserEnrollmentsInCategoryAsync(userId, id));

                if (!canAccess)
                {
                    _logger.LogWarning("Learner {UserId} attempted to access inaccessible category: {CategoryId}", userId, id);
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

                var category = await _categoryService.GetCategoryByIdAsync(id);

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
                // This dynamically gets the ID of the user who is currently logged in.
                var creatorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(creatorId))
                {
                    return Unauthorized(new { message = "User is not authenticated." });
                }

                // It passes the correct user's ID to the service.
                var category = await _categoryService.CreateCategoryAsync(dto, creatorId);
                _logger.LogInformation("Created new category: {CategoryId} - {Title} by User: {UserId}", category.Id, category.Title, creatorId);
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