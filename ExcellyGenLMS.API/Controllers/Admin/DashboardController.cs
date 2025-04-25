using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExcellyGenLMS.Application.DTOs.Admin;
using ExcellyGenLMS.Application.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ExcellyGenLMS.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/dashboard")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IDashboardService dashboardService,
            ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets statistics for the admin dashboard
        /// </summary>
        /// <returns>Dashboard statistics including course categories, users, and technologies</returns>
        [HttpGet("stats")]
        public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats()
        {
            try
            {
                _logger.LogInformation("GET request for dashboard statistics");
                var stats = await _dashboardService.GetDashboardStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard statistics");
                return StatusCode(500, new { message = "An error occurred while retrieving dashboard statistics." });
            }
        }

        /// <summary>
        /// Gets recent notifications for the admin dashboard
        /// </summary>
        /// <returns>List of notifications</returns>
        [HttpGet("notifications")]
        public async Task<ActionResult<List<NotificationDto>>> GetDashboardNotifications()
        {
            try
            {
                _logger.LogInformation("GET request for dashboard notifications");
                var notifications = await _dashboardService.GetDashboardNotificationsAsync();
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard notifications");
                return StatusCode(500, new { message = "An error occurred while retrieving dashboard notifications." });
            }
        }
    }
}