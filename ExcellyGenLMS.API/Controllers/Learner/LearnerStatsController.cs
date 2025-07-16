using ExcellyGenLMS.Application.DTOs;
using ExcellyGenLMS.Application.DTOs.Learner;     // ADDED
using ExcellyGenLMS.Application.Interfaces.Learner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;                    // ADDED
using System.Security.Claims;                        // ADDED
using System.Threading.Tasks;

namespace ExcellyGenLMS.API.Controllers.Learner
{
    [ApiController]
    [Route("api/learner/stats")]
    [Authorize(Roles = "Learner")]
    public class LearnerStatsController : ControllerBase
    {
        private readonly ILearnerStatsService _learnerStatsService;

        public LearnerStatsController(ILearnerStatsService learnerStatsService)
        {
            _learnerStatsService = learnerStatsService;
        }

        [HttpGet("overall")]
        public async Task<ActionResult<OverallLmsStatsDto>> GetOverallLmsStats()
        {
            var stats = await _learnerStatsService.GetOverallLmsStatsAsync();
            return Ok(stats);
        }

        // ADDED THIS ENDPOINT
        [HttpGet("weekly-activity")]
        public async Task<ActionResult<IEnumerable<DailyScreenTimeDto>>> GetWeeklyActivity()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }
            var result = await _learnerStatsService.GetWeeklyScreenTimeAsync(userId);
            return Ok(result);
        }
    }
}