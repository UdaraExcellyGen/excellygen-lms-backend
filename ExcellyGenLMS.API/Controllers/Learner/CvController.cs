using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ExcellyGenLMS.Application.Interfaces.Learner;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using ExcellyGenLMS.Application.DTOs.Learner; // <<<< ADJUSTED using for DTOs new location
using Microsoft.AspNetCore.Http;

namespace ExcellyGenLMS.API.Controllers.Learner // <<<< CHANGED NAMESPACE
{
    [Route("api/learner/[controller]")] // <<<< ADJUSTED ROUTE to reflect Learner context
    [ApiController]
    [Authorize]
    public class CvController : ControllerBase
    {
        private readonly ICvService _cvService;
        private readonly ILogger<CvController> _logger;

        public CvController(ICvService cvService, ILogger<CvController> logger)
        {
            _cvService = cvService;
            _logger = logger;
        }

        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(CvDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCvData(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("GetCvData called with empty userId.");
                return BadRequest(new { message = "User ID cannot be empty." });
            }

            try
            {
                _logger.LogInformation("Requesting CV data for userId: {UserId} via learner/cv endpoint", userId);
                var cvData = await _cvService.GetCvDataAsync(userId);
                return Ok(cvData);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "CV data retrieval failed for userId {UserId}: User profile or related data not found.", userId);
                return NotFound(new { message = ex.Message });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching CV data for userId {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal server error occurred while processing your request." });
            }
        }
    }
}