using ExcellyGenLMS.Application.DTOs.Course;
using ExcellyGenLMS.Application.Interfaces.Course;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ExcellyGenLMS.API.Controllers.Course
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Learner")]
    public class LearnerCoursesController : ControllerBase
    {
        private readonly ILearnerCourseService _learnerCourseService;
        private readonly ILogger<LearnerCoursesController> _logger;

        public LearnerCoursesController(ILearnerCourseService learnerCourseService, ILogger<LearnerCoursesController> logger)
        {
            _learnerCourseService = learnerCourseService ?? throw new ArgumentNullException(nameof(learnerCourseService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private string GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userId))
            {
                var errorMsg = "User ID claim ('NameIdentifier' or 'sub') not found in token.";
                _logger.LogError(errorMsg);
                throw new UnauthorizedAccessException(errorMsg);
            }
            return userId;
        }

        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<LearnerCourseDto>>> GetAvailableCourses([FromQuery] string? categoryId = null)
        {
            // This method remains unchanged
            string userId = GetCurrentUserId();
            var courses = await _learnerCourseService.GetAvailableCoursesAsync(userId, categoryId);
            return Ok(courses);
        }

        [HttpGet("enrolled")]
        public async Task<ActionResult<IEnumerable<LearnerCourseDto>>> GetEnrolledCourses()
        {
            // This method remains unchanged but now returns the new, more detailed DTO
            string userId = GetCurrentUserId();
            var courses = await _learnerCourseService.GetEnrolledCoursesAsync(userId);
            return Ok(courses);
        }

        [HttpGet("{courseId}")]
        public async Task<ActionResult<LearnerCourseDto>> GetLearnerCourseDetails(int courseId)
        {
            // This method remains unchanged but now returns the new, more detailed DTO
            try
            {
                string userId = GetCurrentUserId();
                var courseDetails = await _learnerCourseService.GetLearnerCourseDetailsAsync(userId, courseId);
                if (courseDetails == null)
                {
                    return NotFound(new { message = $"Course with ID {courseId} not found or you are not enrolled." });
                }
                return Ok(courseDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting course details for {CourseId}", courseId);
                return StatusCode(500, new { message = "An internal error occurred." });
            }
        }

        // NEW ENDPOINT: To mark a single document as complete
        [HttpPost("documents/{documentId}/complete")]
        [ProducesResponseType(typeof(DocumentProgressDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DocumentProgressDto>> MarkDocumentCompleted(int documentId)
        {
            try
            {
                string userId = GetCurrentUserId();
                _logger.LogInformation("Marking document {DocumentId} as completed for user {UserId}", documentId, userId);
                var progress = await _learnerCourseService.MarkDocumentCompletedAsync(userId, documentId);
                return Ok(progress);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking document {DocumentId} complete for user {UserId}", documentId, GetCurrentUserId());
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while saving progress." });
            }
        }

        [HttpGet("preview/{courseId}")]
        public async Task<ActionResult<LearnerCourseDto>> GetCoursePreview(int courseId)
        {
            try
            {
                string userId = GetCurrentUserId();
                var coursePreview = await _learnerCourseService.GetCoursePreviewAsync(userId, courseId);
                if (coursePreview == null)
                {
                    return NotFound(new { message = $"Course with ID {courseId} not found or you are already enrolled." });
                }
                return Ok(coursePreview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting course preview for {CourseId}", courseId);
                return StatusCode(500, new { message = "An internal error occurred." });
            }
        }

        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<object>> GetCoursesForCategory(string categoryId)
        {
            try
            {
                string userId = GetCurrentUserId();

                // Get available courses for this category
                var availableCourses = await _learnerCourseService.GetAvailableCoursesAsync(userId, categoryId);

                // Get all enrolled courses, then filter by category
                var allEnrolledCourses = await _learnerCourseService.GetEnrolledCoursesAsync(userId);
                var categoryEnrolledCourses = allEnrolledCourses.Where(c => c.Category?.Id == categoryId);

                return Ok(new
                {
                    available = availableCourses,
                    categoryEnrolled = categoryEnrolledCourses
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting courses for category {CategoryId}", categoryId);
                return StatusCode(500, new { message = "An error occurred while fetching courses." });
            }
        }


    }
}