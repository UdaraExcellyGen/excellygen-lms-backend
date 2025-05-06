using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExcellyGenLMS.Application.DTOs.Learner;
using ExcellyGenLMS.Application.Interfaces.Learner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ExcellyGenLMS.API.Controllers.Learner
{
	[Route("api/user-badges")]
	[ApiController]
	[Authorize]
	public class UserBadgeController : ControllerBase
	{
		private readonly IUserBadgeService _userBadgeService;
		private readonly ILogger<UserBadgeController> _logger;

		public UserBadgeController(
			IUserBadgeService userBadgeService,
			ILogger<UserBadgeController> logger)
		{
			_userBadgeService = userBadgeService;
			_logger = logger;
		}

		[HttpGet("{userId}")]
		public async Task<ActionResult<List<BadgeDto>>> GetUserBadges(string userId)
		{
			try
			{
				_logger.LogInformation("Fetching badges for user {UserId}", userId);
				var badges = await _userBadgeService.GetUserBadgesAsync(userId);
				return Ok(badges);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting user badges: {UserId}", userId);
				return StatusCode(500, new { error = "Failed to retrieve user badges." });
			}
		}

		[HttpGet("{userId}/summary")]
		public async Task<ActionResult<UserBadgeSummaryDto>> GetUserBadgeSummary(string userId)
		{
			try
			{
				_logger.LogInformation("Fetching badge summary for user {UserId}", userId);
				var summary = await _userBadgeService.GetUserBadgeSummaryAsync(userId);
				return Ok(summary);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting user badge summary: {UserId}", userId);
				return StatusCode(500, new { error = "Failed to retrieve user badge summary." });
			}
		}
	}
}