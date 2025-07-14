// ExcellyGenLMS.API/Controllers/course/CourseCoordinatorAnalyticsController.cs
using ExcellyGenLMS.Application.Interfaces.Course;
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

        public CourseCoordinatorAnalyticsController(ICourseCoordinatorAnalyticsService analyticsService, IHttpContextAccessor httpContextAccessor)
        {
            _analyticsService = analyticsService;
            _httpContextAccessor = httpContextAccessor;
        }

        private string? GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        [HttpGet("enrollments")]
        public async Task<IActionResult> GetEnrollmentAnalytics()
        {
            var coordinatorId = GetCurrentUserId();
            if (string.IsNullOrEmpty(coordinatorId))
            {
                return Unauthorized("User ID not found.");
            }
            var data = await _analyticsService.GetEnrollmentAnalyticsAsync(coordinatorId);
            return Ok(data);
        }

        [HttpGet("courses")]
        public async Task<IActionResult> GetCoordinatorCourses()
        {
            var coordinatorId = GetCurrentUserId();
            if (string.IsNullOrEmpty(coordinatorId))
            {
                return Unauthorized("User ID not found.");
            }
            var data = await _analyticsService.GetCoordinatorCoursesAsync(coordinatorId);
            return Ok(data);
        }

        [HttpGet("courses/{courseId}/quizzes")]
        public async Task<IActionResult> GetQuizzesForCourse(int courseId)
        {
            var coordinatorId = GetCurrentUserId();
            if (string.IsNullOrEmpty(coordinatorId))
            {
                return Unauthorized("User ID not found.");
            }
            var data = await _analyticsService.GetQuizzesForCourseAsync(courseId, coordinatorId);
            return Ok(data);
        }

        [HttpGet("quizzes/{quizId}/performance")]
        public async Task<IActionResult> GetQuizPerformance(int quizId)
        {
            var coordinatorId = GetCurrentUserId();
            if (string.IsNullOrEmpty(coordinatorId))
            {
                return Unauthorized("User ID not found.");
            }
            var data = await _analyticsService.GetQuizPerformanceAsync(quizId, coordinatorId);
            return Ok(data);
        }
    }
}