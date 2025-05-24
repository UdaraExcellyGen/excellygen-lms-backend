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
    [Route("api/user-certifications")]
    [ApiController]
    [Authorize]
    public class UserCertificationController : ControllerBase
    {
        private readonly IUserCertificationService _userCertificationService;
        private readonly ILogger<UserCertificationController> _logger;

        public UserCertificationController(
            IUserCertificationService userCertificationService,
            ILogger<UserCertificationController> logger)
        {
            _userCertificationService = userCertificationService;
            _logger = logger;
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<List<CertificationDto>>> GetUserCertifications(string userId)
        {
            try
            {
                _logger.LogInformation("Fetching certifications for user {UserId}", userId);
                var certifications = await _userCertificationService.GetUserCertificationsAsync(userId);
                return Ok(certifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user certifications: {UserId}", userId);
                return StatusCode(500, new { error = "Failed to retrieve user certifications." });
            }
        }

        [HttpGet("{userId}/{certificationId}")]
        public async Task<ActionResult<CertificationDto>> GetUserCertification(string userId, string certificationId)
        {
            try
            {
                _logger.LogInformation("Fetching certification {CertificationId} for user {UserId}", certificationId, userId);
                var certification = await _userCertificationService.GetUserCertificationByIdAsync(userId, certificationId);
                return Ok(certification);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Certification not found: {UserId}, {CertificationId}", userId, certificationId);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user certification: {UserId}, {CertificationId}", userId, certificationId);
                return StatusCode(500, new { error = "Failed to retrieve user certification." });
            }
        }
    }
}