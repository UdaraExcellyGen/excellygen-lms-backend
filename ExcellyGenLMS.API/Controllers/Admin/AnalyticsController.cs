using System;
using System.Threading.Tasks;
using ExcellyGenLMS.Application.DTOs.Admin;
using ExcellyGenLMS.Application.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ExcellyGenLMS.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/analytics")]
    [Authorize(Roles = "Admin")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(
            IAnalyticsService analyticsService,
            ILogger<AnalyticsController> logger)
        {
            _analyticsService = analyticsService;
            _logger = logger;
        }

        [HttpGet("kpi-summary")]
        public async Task<ActionResult<KpiSummaryDto>> GetKpiSummary()
        {
            try
            {
                _logger.LogInformation("Getting KPI summary.");
                var summary = await _analyticsService.GetKpiSummaryAsync();
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting KPI summary.");
                return StatusCode(500, new { message = "An error occurred while retrieving KPI summary data." });
            }
        }

        [HttpGet("enrollment-kpis")]
        public async Task<ActionResult<EnrollmentKpiDto>> GetEnrollmentKpis()
        {
            try
            {
                _logger.LogInformation("Getting enrollment KPIs.");
                var kpis = await _analyticsService.GetEnrollmentKpiAsync();
                return Ok(kpis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting enrollment KPIs.");
                return StatusCode(500, new { message = "An error occurred while retrieving enrollment KPI data." });
            }
        }

        [HttpGet("enrollment")]
        public async Task<ActionResult<EnrollmentAnalyticsDto>> GetEnrollmentAnalytics([FromQuery] string? categoryId = null)
        {
            try
            {
                _logger.LogInformation("Getting enrollment analytics for category: {CategoryId}", categoryId ?? "all");
                var analytics = await _analyticsService.GetEnrollmentAnalyticsAsync(categoryId);
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting enrollment analytics");
                return StatusCode(500, new { message = "An error occurred while retrieving enrollment analytics data." });
            }
        }

        [HttpGet("course-availability")]
        public async Task<ActionResult<CourseAvailabilityDto>> GetCourseAvailabilityAnalytics()
        {
            try
            {
                _logger.LogInformation("Getting course availability analytics");
                var analytics = await _analyticsService.GetCourseAvailabilityAnalyticsAsync();
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting course availability analytics");
                return StatusCode(500, new { message = "An error occurred while retrieving course availability analytics data." });
            }
        }

        [HttpGet("user-distribution")]
        public async Task<ActionResult<UserDistributionDto>> GetUserDistributionAnalytics()
        {
            try
            {
                _logger.LogInformation("Getting user distribution analytics");
                var analytics = await _analyticsService.GetUserDistributionAnalyticsAsync();
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user distribution analytics");
                return StatusCode(500, new { message = "An error occurred while retrieving user distribution analytics data." });
            }
        }
    }
}