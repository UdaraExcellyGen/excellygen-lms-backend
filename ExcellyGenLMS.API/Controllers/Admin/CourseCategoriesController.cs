using Microsoft.AspNetCore.Mvc;
using ExcellyGenLMS.Application.DTOs.Admin;
using ExcellyGenLMS.Application.DTOs.Course;
using ExcellyGenLMS.Application.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace ExcellyGenLMS.API.Controllers.Admin
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
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

        [HttpGet]
        public async Task<ActionResult<List<CourseCategoryDto>>> GetAllCategories()
        {
            try
            {
                _logger.LogInformation("Getting all course categories");
                var categories = await _categoryService.GetAllCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all categories");
                return StatusCode(500, new { message = "An error occurred while retrieving categories." });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CourseCategoryDto>> GetCategoryById(string id)
        {
            try
            {
                _logger.LogInformation("Getting course category with ID: {CategoryId}", id);
                var category = await _categoryService.GetCategoryByIdAsync(id);
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

        [HttpPost]
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

        [HttpGet("{categoryId}/courses")]
        public async Task<ActionResult<List<CourseDto>>> GetCoursesByCategory(string categoryId)
        {
            try
            {
                _logger.LogInformation("Getting courses for category with ID: {CategoryId}", categoryId);

                // First verify the category exists
                await _categoryService.GetCategoryByIdAsync(categoryId);

                // Get courses for this category
                var courses = await _courseService.GetCoursesByCategoryIdAsync(categoryId);
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
    }
}