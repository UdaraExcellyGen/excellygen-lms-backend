using ExcellyGenLMS.Application.Interfaces.Course;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System;

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

        [HttpGet("enrollments")]
        public async Task<IActionResult> GetEnrollmentAnalytics(
            [FromQuery] string? categoryId = null,
            [FromQuery] string status = "all",
            [FromQuery] string ownership = "mine")
        {
            var coordinatorId = GetCurrentUserId();
            if (string.IsNullOrEmpty(coordinatorId))
            {
                return Unauthorized("User ID not found.");
            }

            try
            {
                var data = await _analyticsService.GetEnrollmentAnalyticsAsync(coordinatorId, categoryId, status, ownership);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

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

        [HttpGet("courses")]
        public async Task<IActionResult> GetCoordinatorCourses(
            [FromQuery] string? categoryId = null,
            [FromQuery] string ownership = "mine")
        {
            var coordinatorId = GetCurrentUserId();
            if (string.IsNullOrEmpty(coordinatorId))
            {
                return Unauthorized("User ID not found.");
            }

            try
            {
                var data = await _analyticsService.GetCoordinatorCoursesAsync(coordinatorId, categoryId, ownership);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

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