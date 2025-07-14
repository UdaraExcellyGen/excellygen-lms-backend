using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ExcellyGenLMS.Application.DTOs.Learner;
using ExcellyGenLMS.Application.Interfaces.Learner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ExcellyGenLMS.API.Controllers.Learner
{
    [Route("api/leaderboard")]
    [ApiController]
    [Authorize]
    public class LeaderboardController : ControllerBase
    {
        private readonly ILeaderboardService _leaderboardService;
        private readonly ILogger<LeaderboardController> _logger;

        public LeaderboardController(ILeaderboardService leaderboardService, ILogger<LeaderboardController> logger)
        {
            _leaderboardService = leaderboardService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<LeaderboardDto>> GetLeaderboard()
        {
            try
            {
                // Retrieve the user ID from the JWT token claims
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in token.");
                    return Unauthorized(new { error = "User ID not found in token." });
                }

                _logger.LogInformation("Fetching leaderboard for user {UserId}", userId);
                var leaderboard = await _leaderboardService.GetLeaderboardAsync(userId);
                return Ok(leaderboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting the leaderboard.");
                return StatusCode(500, new { error = "An internal server error occurred." });
            }
        }
    }
}