using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using ExcellyGenLMS.Application.DTOs.Admin;     // For UpdateCourseAdminDto
using ExcellyGenLMS.Application.DTOs.Course;     // For CourseDto (return type)
using ExcellyGenLMS.Application.Interfaces.Admin; // For ICourseAdminService
using System;                                   // For ArgumentNullException, Exception
using System.Collections.Generic;               // For KeyNotFoundException, List<> (if GET added)
using System.Threading.Tasks;                   // For Task, async/await
using Microsoft.AspNetCore.Http;                // For StatusCodes

namespace ExcellyGenLMS.API.Controllers.Admin
{
    /// <summary>
    /// API Controller for Admin-specific operations on courses.
    /// Allows Admins to update certain fields or delete courses.
    /// All actions require the "Admin" role.
    /// </summary>
    [ApiController]
    [Route("api/admin/courses")] // Route specifically for admin course operations
    [Authorize(Roles = "Admin")] // Ensures only users with the Admin role can access these endpoints
    public class CoursesAdminController : ControllerBase
    {
        private readonly ICourseAdminService _courseAdminService;
        private readonly ILogger<CoursesAdminController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoursesAdminController"/> class.
        /// </summary>
        /// <param name="courseAdminService">The admin course service.</param>
        /// <param name="logger">The logger.</param>
        public CoursesAdminController(ICourseAdminService courseAdminService, ILogger<CoursesAdminController> logger)
        {
            _courseAdminService = courseAdminService ?? throw new ArgumentNullException(nameof(courseAdminService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Updates specific fields of a course (e.g., Title, Description) by an Admin.
        /// </summary>
        /// <param name="id">The integer ID of the course to update.</param>
        /// <param name="updateCourseDto">The DTO containing the fields to update.</param>
        /// <returns>An ActionResult containing the updated CourseDto on success, or an error status code.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(CourseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CourseDto>> UpdateCourse(int id, [FromBody] UpdateCourseAdminDto updateCourseDto)
        {
            // Optional: Basic model state validation based on DTO attributes
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Admin UpdateCourse failed model validation for ID: {CourseId}", id);
                return BadRequest(ModelState);
            }

            try
            {
                _logger.LogInformation("Admin attempting to update course with ID: {CourseId}", id);
                // Call the service method specifically designed for Admin updates
                var updatedCourse = await _courseAdminService.UpdateCourseAdminAsync(id, updateCourseDto);
                _logger.LogInformation("Admin successfully updated course {CourseId}.", id);
                return Ok(updatedCourse); // Return 200 OK with the updated data
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Admin update failed: Course not found: {CourseId}", id);
                // Return 404 Not Found with the specific error message
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex) // Catch validation errors thrown by the service
            {
                _logger.LogWarning(ex, "Admin update failed: Invalid argument/data for course {CourseId}: {ErrorMessage}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex) // Catch business rule violations
            {
                _logger.LogWarning(ex, "Admin update failed: Operation not allowed for course {CourseId}: {ErrorMessage}", id, ex.Message);
                return BadRequest(new { message = ex.Message }); // 400 Bad Request might be suitable
            }
            catch (Exception ex) // Catch any other unexpected errors
            {
                _logger.LogError(ex, "Unexpected error updating course by Admin: {CourseId}", id);
                // Return 500 Internal Server Error with a generic message
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred while updating the course." });
            }
        }

        /// <summary>
        /// Deletes a course by its ID. Performed by an Admin.
        /// Note: This might have different side effects or checks compared to a Coordinator deleting a course.
        /// </summary>
        /// <param name="id">The integer ID of the course to delete.</param>
        /// <returns>An IActionResult indicating success (204 No Content) or an error status code.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            try
            {
                _logger.LogInformation("Admin attempting to delete course with ID: {CourseId}", id);
                // Call the service method specifically designed for Admin deletion
                await _courseAdminService.DeleteCourseAsync(id);
                _logger.LogInformation("Admin successfully deleted course {CourseId}.", id);
                // HTTP 204 No Content is the standard success response for DELETE operations
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Admin delete failed: Course not found: {CourseId}", id);
                // Return 404 Not Found
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex) // Catch business rule violations if delete isn't allowed
            {
                _logger.LogWarning(ex, "Admin delete failed: Operation not allowed for course {CourseId}: {ErrorMessage}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex) // Catch unexpected errors (e.g., database constraints, file system errors if applicable)
            {
                _logger.LogError(ex, "Unexpected error deleting course by Admin: {CourseId}", id);
                // Return 500 Internal Server Error
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred while deleting the course." });
            }
        }

        // Potential Future GET Endpoint (Example)
        // GET /api/admin/courses?categoryId=someGuid
        /*
        [HttpGet]
        [ProducesResponseType(typeof(List<CourseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<CourseDto>>> GetCoursesForAdmin([FromQuery] string categoryId)
        {
            if (string.IsNullOrWhiteSpace(categoryId))
            {
                 _logger.LogWarning("Admin GetCourses request received without categoryId.");
                return BadRequest(new { message = "categoryId query parameter is required." });
            }

            try
            {
                _logger.LogInformation("Admin retrieving courses for category ID: {CategoryId}", categoryId);
                var courses = await _courseAdminService.GetCoursesByCategoryIdAsync(categoryId);
                return Ok(courses);
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Unexpected error retrieving courses for admin with CategoryId {CategoryId}", categoryId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }
        */

    }
}