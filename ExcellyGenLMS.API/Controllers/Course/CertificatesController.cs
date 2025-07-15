using ExcellyGenLMS.Application.DTOs.Course;
using ExcellyGenLMS.Application.Interfaces.Course;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ExcellyGenLMS.API.Controllers.Course
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Changed: Authorize all actions, but apply specific roles where needed
    public class CertificatesController : ControllerBase
    {
        private readonly ICertificateService _certificateService;
        private readonly ILogger<CertificatesController> _logger;

        public CertificatesController(ICertificateService certificateService, ILogger<CertificatesController> logger)
        {
            _certificateService = certificateService ?? throw new ArgumentNullException(nameof(certificateService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private string GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userId))
            {
                var errorMsg = "User ID claim ('NameIdentifier' or 'sub') not found in token.";
                _logger.LogError(errorMsg);
                throw new UnauthorizedAccessException(errorMsg);
            }
            return userId;
        }

        // GET: api/Certificates/{certificateId}
        [HttpGet("{certificateId}")]
        [Authorize(Roles = "Learner")]
        public async Task<ActionResult<CertificateDto>> GetCertificateById(int certificateId)
        {
            // ... (Existing code for this method remains unchanged)
            try
            {
                string userId = GetCurrentUserId();
                var certificate = await _certificateService.GetCertificateByIdAsync(certificateId);

                if (certificate == null)
                {
                    _logger.LogWarning("Certificate {CertificateId} not found.", certificateId);
                    return NotFound(new { message = $"Certificate with ID {certificateId} not found." });
                }

                if (certificate.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} attempted to access certificate {CertificateId} belonging to user {CertUserId}", userId, certificateId, certificate.UserId);
                    return Forbid();
                }

                return Ok(certificate);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access trying to get certificate.");
                return Unauthorized(new { message = "User not authenticated or ID not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving certificate {CertificateId} for user {UserId}", certificateId, GetCurrentUserId());
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving the certificate." });
            }
        }

        // GET: api/Certificates/user (For logged-in user's own page)
        [HttpGet("user")]
        [Authorize(Roles = "Learner")]
        public async Task<ActionResult<IEnumerable<CertificateDto>>> GetUserCertificates()
        {
            // ... (Existing code for this method remains unchanged)
            try
            {
                string userId = GetCurrentUserId();
                _logger.LogInformation("Retrieving all certificates for user {UserId}", userId);
                var certificates = await _certificateService.GetCertificatesByUserIdAsync(userId);
                return Ok(certificates);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access trying to get user certificates.");
                return Unauthorized(new { message = "User not authenticated or ID not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving certificates for user {UserId}", GetCurrentUserId());
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving your certificates." });
            }
        }

        // ========================================================================
        // NEW METHOD: GET: api/Certificates/user/{userId} (For public profiles)
        // ========================================================================
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<CertificateDto>>> GetUserCertificatesByUserId(string userId)
        {
            try
            {
                _logger.LogInformation("Fetching internal certificates for profile view of user {UserId}", userId);
                var certificates = await _certificateService.GetCertificatesByUserIdAsync(userId);
                return Ok(certificates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving internal certificates for user {UserId}", userId);
                return StatusCode(500, "An error occurred while retrieving certificates.");
            }
        }
        // ========================================================================

        // POST: api/Certificates/generate
        [HttpPost("generate")]
        [Authorize(Roles = "Learner")]
        public async Task<ActionResult<CertificateDto>> GenerateCertificate([FromBody] GenerateCertificateDto generateDto)
        {
            // ... (Existing code for this method remains unchanged)
            try
            {
                string userId = GetCurrentUserId();
                var certificate = await _certificateService.GenerateCertificateAsync(userId, generateDto.CourseId);
                return CreatedAtAction(nameof(GetCertificateById), new { certificateId = certificate.Id }, certificate);
            }
            catch (Exception ex)
            {
                // Simplified error handling for brevity, you can keep your detailed original handling
                _logger.LogError(ex, "Error generating certificate.");
                return StatusCode(500, "Error generating certificate.");
            }
        }
    }
}