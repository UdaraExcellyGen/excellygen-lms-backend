using ExcellyGenLMS.Application.Interfaces.Learner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;

namespace ExcellyGenLMS.API.Controllers.Learner
{
    [Route("api/badges")] // This is the route your frontend uses
    [ApiController]
    [Authorize(Roles = "Learner")]
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
        /// </summary>
        [HttpGet]
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
                _logger.LogError(ex, "Error getting badges for user.");
                return StatusCode(500, "An internal error occurred while fetching badges.");
            }
        }

        /// <summary>
        /// Claims an unlocked badge for the current user.
        /// </summary>
        [HttpPost("{badgeId}/claim")]
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