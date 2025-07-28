using Microsoft.AspNetCore.Mvc;
using ExcellyGenLMS.Application.DTOs.Admin;
using ExcellyGenLMS.Application.DTOs.Course; // THIS USING STATEMENT IS NOW CORRECT AND NECESSARY
using ExcellyGenLMS.Application.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
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

        [HttpGet("{categoryId}/courses")]
        [Authorize(Roles = "Admin")]
        // THE CRITICAL FIX: Changed the incorrect 'CourseAdminDto' to the correct 'CourseDto'
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetCoursesForCategory(string categoryId)
        {
            try
            {
                _logger.LogInformation("Getting courses for category ID: {CategoryId}", categoryId);
                var courses = await _courseService.GetCoursesByCategoryIdAsync(categoryId);
                return Ok(courses);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Category not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting courses for category {CategoryId}", categoryId);
                return StatusCode(500, "An internal error occurred");
            }
        }

        // --- ALL OTHER METHODS ARE CONFIRMED CORRECT AND UNCHANGED ---
        [HttpGet]
        [Authorize(Roles = "Admin,Learner")]
        public async Task<ActionResult<List<CourseCategoryDto>>> GetAllCategories([FromQuery] bool includeDeleted = false) { try { var userIsAdmin = User.IsInRole("Admin"); var allCategories = await _categoryService.GetAllCategoriesAsync(userIsAdmin && includeDeleted); if (userIsAdmin) { return Ok(allCategories); } var learnerVisibleCategories = allCategories.Where(c => !c.IsDeleted && c.Status.Equals("active", StringComparison.OrdinalIgnoreCase)).ToList(); return Ok(learnerVisibleCategories); } catch (Exception ex) { _logger.LogError(ex, "Error getting all categories"); return StatusCode(500, new { message = "An error occurred." }); } }
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Learner")]
        public async Task<ActionResult<CourseCategoryDto>> GetCategoryById(string id) { try { var category = await _categoryService.GetCategoryByIdAsync(id); if (!User.IsInRole("Admin") && (category.IsDeleted || !category.Status.Equals("active", StringComparison.OrdinalIgnoreCase))) { return NotFound(new { message = "Course category not found." }); } return Ok(category); } catch (KeyNotFoundException) { return NotFound(new { message = $"Category with ID {id} not found." }); } catch (Exception ex) { _logger.LogError(ex, "Error getting category: {CategoryId}", id); return StatusCode(500, new { message = "An error occurred." }); } }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CourseCategoryDto>> CreateCategory([FromBody] CreateCourseCategoryDto createCategoryDto) { try { var creatorId = User.FindFirstValue(ClaimTypes.NameIdentifier); if (string.IsNullOrEmpty(creatorId)) { return Unauthorized("User ID claim not found in token."); } var category = await _categoryService.CreateCategoryAsync(createCategoryDto, creatorId); return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category); } catch (Exception ex) { _logger.LogError(ex, "Error creating category"); return StatusCode(500, new { message = "An error occurred." }); } }
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CourseCategoryDto>> UpdateCategory(string id, [FromBody] UpdateCourseCategoryDto dto) { try { return Ok(await _categoryService.UpdateCategoryAsync(id, dto)); } catch (KeyNotFoundException) { return NotFound(new { message = $"Category with ID {id} not found." }); } catch (Exception ex) { _logger.LogError(ex, "Error updating category: {CategoryId}", id); return StatusCode(500, new { message = "An error occurred." }); } }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteCategory(string id) { try { await _categoryService.DeleteCategoryAsync(id); return NoContent(); } catch (KeyNotFoundException) { return NotFound(new { message = $"Category with ID {id} not found." }); } catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); } catch (Exception ex) { _logger.LogError(ex, "Error deleting category: {CategoryId}", id); return StatusCode(500, new { message = "An error occurred." }); } }
        [HttpPost("{id}/restore")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CourseCategoryDto>> RestoreCategory(string id) { try { return Ok(await _categoryService.RestoreCategoryAsync(id)); } catch (KeyNotFoundException) { return NotFound(new { message = $"Category with ID {id} not found." }); } catch (Exception ex) { _logger.LogError(ex, "Error restoring category: {CategoryId}", id); return StatusCode(500, new { message = "An error occurred." }); } }
        [HttpPatch("{id}/toggle-status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CourseCategoryDto>> ToggleCategoryStatus(string id) { try { return Ok(await _categoryService.ToggleCategoryStatusAsync(id)); } catch (KeyNotFoundException) { return NotFound(new { message = $"Category with ID {id} not found." }); } catch (Exception ex) { _logger.LogError(ex, "Error toggling status: {CategoryId}", id); return StatusCode(500, new { message = "An error occurred." }); } }
    }
}