// ExcellyGenLMS.API/Controllers/Learner/BadgesController.cs

using ExcellyGenLMS.Application.Interfaces.Learner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using ExcellyGenLMS.Application.DTOs.Learner;

namespace ExcellyGenLMS.API.Controllers.Learner
{
    [Route("api/badges")]
    [ApiController]
    [Authorize] // General authorization for the controller
    public class BadgesController : ControllerBase
    {
        private readonly IBadgesAndRewardsService _badgesService;
        private readonly ILogger<BadgesController> _logger;

        public BadgesController(IBadgesAndRewardsService badgesService, ILogger<BadgesController> logger)
        {
            _badgesService = badgesService;
            _logger = logger;
        }

        private string GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token.");
            }
            return userId;
        }

        /// <summary>
        /// Gets all badges and the current user's progress towards them.
        /// Intended for the "My Badges & Rewards" page.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Learner")] // Specific to Learner role
        public async Task<IActionResult> GetBadgesAndRewards()
        {
            try
            {
                var userId = GetCurrentUserId();
                var badges = await _badgesService.GetBadgesAndRewardsAsync(userId);
                return Ok(badges);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting badges for current user.");
                return StatusCode(500, "An internal error occurred while fetching badges.");
            }
        }

        // =================================================================
        // NEW METHOD to get badges for a specific user (for public profiles)
        // =================================================================
        /// <summary>
        /// Gets badges and progress for a specific user by their ID.
        /// </summary>
        /// <param name="userId">The ID of the user whose badges to retrieve.</param>
        [HttpGet("{userId}")]
        public async Task<ActionResult<IEnumerable<BadgeDto>>> GetBadgesForUser(string userId)
        {
            try
            {
                _logger.LogInformation("Fetching badges for user {UserId}", userId);
                var badges = await _badgesService.GetBadgesAndRewardsAsync(userId);
                return Ok(badges);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting badges for user {UserId}", userId);
                return StatusCode(500, new { error = "An internal error occurred while fetching badges for the specified user." });
            }
        }
        // =================================================================

        /// <summary>
        /// Claims an unlocked badge for the current user.
        /// </summary>
        [HttpPost("{badgeId}/claim")]
        [Authorize(Roles = "Learner")] // Specific to Learner role
        public async Task<IActionResult> ClaimBadge(string badgeId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var claimedBadge = await _badgesService.ClaimBadgeAsync(userId, badgeId);
                return Ok(claimedBadge);
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
                _logger.LogError(ex, "Error claiming badge {BadgeId} for user.", badgeId);
                return StatusCode(500, "An internal error occurred while claiming the badge.");
            }
        }
    }
}