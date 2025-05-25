// ExcellyGenLMS.API/Controllers/Course/LearnerCoursesController.cs
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
    [Authorize(Roles = "Learner")] // Only learners should access these
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

        // GET: api/LearnerCourses/available
        [HttpGet("available")]
        [ProducesResponseType(typeof(IEnumerable<LearnerCourseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<LearnerCourseDto>>> GetAvailableCourses([FromQuery] string? categoryId = null)
        {
            try
            {
                string userId = GetCurrentUserId();
                _logger.LogInformation("Getting available courses for user {UserId}, category: {CategoryId}", userId, categoryId ?? "All");
                var courses = await _learnerCourseService.GetAvailableCoursesAsync(userId, categoryId);
                return Ok(courses);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access trying to get available courses.");
                return Unauthorized(new { message = "User not authenticated or ID not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available courses for user {UserId}", GetCurrentUserId());
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving available courses." });
            }
        }

        // GET: api/LearnerCourses/enrolled
        [HttpGet("enrolled")]
        [ProducesResponseType(typeof(IEnumerable<LearnerCourseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<LearnerCourseDto>>> GetEnrolledCourses()
        {
            try
            {
                string userId = GetCurrentUserId();
                _logger.LogInformation("Getting enrolled courses for user {UserId}", userId);
                var courses = await _learnerCourseService.GetEnrolledCoursesAsync(userId);
                return Ok(courses);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access trying to get enrolled courses.");
                return Unauthorized(new { message = "User not authenticated or ID not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting enrolled courses for user {UserId}", GetCurrentUserId());
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving enrolled courses." });
            }
        }

        // GET: api/LearnerCourses/{courseId}
        [HttpGet("{courseId}")]
        [ProducesResponseType(typeof(LearnerCourseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LearnerCourseDto>> GetLearnerCourseDetails(int courseId)
        {
            try
            {
                string userId = GetCurrentUserId();
                _logger.LogInformation("Getting details for course {CourseId} for learner {UserId}", courseId, userId);
                var courseDetails = await _learnerCourseService.GetLearnerCourseDetailsAsync(userId, courseId);
                if (courseDetails == null)
                {
                    _logger.LogWarning("Course {CourseId} not found for learner {UserId}", courseId, userId);
                    return NotFound(new { message = $"Course with ID {courseId} not found." });
                }
                return Ok(courseDetails);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access trying to get learner course details.");
                return Unauthorized(new { message = "User not authenticated or ID not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting learner course details for user {UserId} and course {CourseId}", GetCurrentUserId(), courseId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving course details." });
            }
        }

        // PATCH: api/LearnerCourses/lessons/{lessonId}/complete
        [HttpPatch("lessons/{lessonId}/complete")]
        [ProducesResponseType(typeof(LessonProgressDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LessonProgressDto>> MarkLessonCompleted(int lessonId)
        {
            try
            {
                string userId = GetCurrentUserId();
                _logger.LogInformation("Marking lesson {LessonId} as completed for user {UserId}", lessonId, userId);
                var progress = await _learnerCourseService.MarkLessonCompletedAsync(userId, lessonId);
                return Ok(progress);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access trying to mark lesson as complete.");
                return Unauthorized(new { message = "User not authenticated or ID not found." });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Lesson {LessonId} not found when marking complete for user {UserId}", lessonId, GetCurrentUserId());
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking lesson {LessonId} complete for user {UserId}", lessonId, GetCurrentUserId());
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while marking the lesson complete." });
            }
        }
    }
}