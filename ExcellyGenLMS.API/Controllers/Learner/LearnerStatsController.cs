// ExcellyGenLMS.API/Controllers/Learner/LearnerStatsController.cs
using ExcellyGenLMS.Application.DTOs;
using ExcellyGenLMS.Application.Interfaces.Learner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    }
}