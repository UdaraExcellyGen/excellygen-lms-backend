using Microsoft.AspNetCore.Mvc;
using ExcellyGenLMS.Application.DTOs.Course;
using ExcellyGenLMS.Application.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace ExcellyGenLMS.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/courses")]
    [Authorize(Roles = "Admin")]
    public class CoursesAdminController : ControllerBase
    {
        private readonly ICourseAdminService _courseService;
        private readonly ILogger<CoursesAdminController> _logger;

        public CoursesAdminController(ICourseAdminService courseService, ILogger<CoursesAdminController> logger)
        {
            _courseService = courseService;
            _logger = logger;
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CourseDto>> UpdateCourse(int id, [FromBody] UpdateCourseAdminDto updateCourseDto)
        {
            try
            {
                _logger.LogInformation("Admin updating course with ID: {CourseId}", id);
                var course = await _courseService.UpdateCourseAdminAsync(id, updateCourseDto);
                return Ok(course);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Course not found: {CourseId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating course: {CourseId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the course." });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCourse(int id)
        {
            try
            {
                _logger.LogInformation("Admin deleting course with ID: {CourseId}", id);
                await _courseService.DeleteCourseAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Course not found: {CourseId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting course: {CourseId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the course." });
            }
        }
    }
}