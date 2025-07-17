// ExcellyGenLMS.API/Controllers/Course/CourseCoordinatorAnalyticsController.cs
using ExcellyGenLMS.Application.Interfaces.Course;
using ExcellyGenLMS.Application.DTOs.Course;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ExcellyGenLMS.API.Controllers.Course
{
    [Authorize(Roles = "CourseCoordinator")]
    [Route("api/coordinator-analytics")]
    [ApiController]
    public class CourseCoordinatorAnalyticsController : ControllerBase
    {
        private readonly ICourseCoordinatorAnalyticsService _analyticsService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CourseCoordinatorAnalyticsController(
            ICourseCoordinatorAnalyticsService analyticsService,
            IHttpContextAccessor httpContextAccessor)
        {
            _analyticsService = analyticsService;
            _httpContextAccessor = httpContextAccessor;
        }

        private string? GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        /// <summary>
        /// Get enrollment analytics with optional category and status filtering
        /// </summary>
        [HttpGet("enrollments")]
        public async Task<IActionResult> GetEnrollmentAnalytics(
            [FromQuery] string? categoryId = null,
            [FromQuery] string status = "all")
        {
            var coordinatorId = GetCurrentUserId();
            if (string.IsNullOrEmpty(coordinatorId))
            {
                return Unauthorized("User ID not found.");
            }

            try
            {
                var data = await _analyticsService.GetEnrollmentAnalyticsAsync(coordinatorId, categoryId, status);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get all course categories available to the coordinator
        /// </summary>
        [HttpGet("categories")]
        public async Task<IActionResult> GetCourseCategories()
        {
            var coordinatorId = GetCurrentUserId();
            if (string.IsNullOrEmpty(coordinatorId))
            {
                return Unauthorized("User ID not found.");
            }

            try
            {
                var categories = await _analyticsService.GetCourseCategoriesAsync(coordinatorId);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get courses created by current coordinator with optional category filtering
        /// </summary>
        [HttpGet("courses")]
        public async Task<IActionResult> GetCoordinatorCourses(
            [FromQuery] string? categoryId = null)
        {
            var coordinatorId = GetCurrentUserId();
            if (string.IsNullOrEmpty(coordinatorId))
            {
                return Unauthorized("User ID not found.");
            }

            try
            {
                var data = await _analyticsService.GetCoordinatorCoursesAsync(coordinatorId, categoryId);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get quizzes for a specific course (only coordinator's courses)
        /// </summary>
        [HttpGet("courses/{courseId}/quizzes")]
        public async Task<IActionResult> GetQuizzesForCourse(int courseId)
        {
            var coordinatorId = GetCurrentUserId();
            if (string.IsNullOrEmpty(coordinatorId))
            {
                return Unauthorized("User ID not found.");
            }

            try
            {
                var data = await _analyticsService.GetQuizzesForCourseAsync(courseId, coordinatorId);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get quiz performance with mark intervals
        /// </summary>
        [HttpGet("quizzes/{quizId}/performance")]
        public async Task<IActionResult> GetQuizPerformance(int quizId)
        {
            var coordinatorId = GetCurrentUserId();
            if (string.IsNullOrEmpty(coordinatorId))
            {
                return Unauthorized("User ID not found.");
            }

            try
            {
                var data = await _analyticsService.GetQuizPerformanceAsync(quizId, coordinatorId);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}