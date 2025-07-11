// Path: ExcellyGenLMS.API/Controllers/Learner/LearnerNotificationController.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ExcellyGenLMS.Application.DTOs.Learner;
using ExcellyGenLMS.Application.Interfaces.Learner;
using System.Security.Claims;

namespace ExcellyGenLMS.API.Controllers.Learner
{
    [ApiController]
    [Route("api/learner-notifications")]
    [Authorize]
    public class LearnerNotificationController : ControllerBase
    {
        private readonly ILearnerNotificationService _notificationService;
        private readonly ILogger<LearnerNotificationController> _logger;

        public LearnerNotificationController(
            ILearnerNotificationService notificationService,
            ILogger<LearnerNotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpGet("summary")]
        public async Task<ActionResult<LearnerNotificationSummaryDto>> GetNotificationSummary()
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation($"Getting notification summary for user: {userId}");

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in token");
                    return Unauthorized("User ID not found in token");
                }

                var summary = await _notificationService.GetUserNotificationSummaryAsync(userId);
                _logger.LogInformation($"Found {summary.TotalCount} total notifications, {summary.UnreadCount} unread for user {userId}");

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification summary");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LearnerNotificationDto>>> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation($"Getting notifications for user: {userId}, page: {page}, pageSize: {pageSize}");

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in token");
                    return Unauthorized("User ID not found in token");
                }

                var notifications = await _notificationService.GetUserNotificationsAsync(userId, page, pageSize);
                _logger.LogInformation($"Found {notifications.Count()} notifications for user {userId}");

                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("unread-count")]
        public async Task<ActionResult<int>> GetUnreadCount()
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation($"Getting unread count for user: {userId}");

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in token");
                    return Unauthorized("User ID not found in token");
                }

                var count = await _notificationService.GetUnreadNotificationCountAsync(userId);
                _logger.LogInformation($"Unread count for user {userId}: {count}");

                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}/mark-read")]
        public async Task<ActionResult> MarkAsRead(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User ID not found in token");
                }

                var success = await _notificationService.MarkNotificationAsReadAsync(id, userId);
                if (!success)
                {
                    return NotFound("Notification not found or access denied");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("mark-all-read")]
        public async Task<ActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User ID not found in token");
                }

                var success = await _notificationService.MarkAllNotificationsAsReadAsync(userId);
                return Ok(new { success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteNotification(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User ID not found in token");
                }

                var success = await _notificationService.DeleteNotificationAsync(id, userId);
                if (!success)
                {
                    return NotFound("Notification not found or access denied");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification");
                return StatusCode(500, "Internal server error");
            }
        }

        private string? GetCurrentUserId()
        {
            // Try multiple claim types to get user ID
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                userId = User.FindFirst("sub")?.Value;
            }

            if (string.IsNullOrEmpty(userId))
            {
                userId = User.FindFirst("user_id")?.Value;
            }

            if (string.IsNullOrEmpty(userId))
            {
                userId = User.FindFirst("UserId")?.Value;
            }

            _logger.LogInformation($"Extracted User ID: {userId}");
            _logger.LogInformation($"All claims: {string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}"))}");

            return userId;
        }
    }
}