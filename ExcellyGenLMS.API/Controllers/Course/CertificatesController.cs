// ExcellyGenLMS.API/Controllers/Course/CertificatesController.cs
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
    [Authorize(Roles = "Learner")]
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
        [ProducesResponseType(typeof(CertificateDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CertificateDto>> GetCertificateById(int certificateId)
        {
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
                    return Forbid(); // Changed from Forbid(new { message = "..." }) to Forbid()
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

        // GET: api/Certificates/user
        [HttpGet("user")]
        [ProducesResponseType(typeof(IEnumerable<CertificateDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<CertificateDto>>> GetUserCertificates()
        {
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

        // POST: api/Certificates/generate
        [HttpPost("generate")]
        [ProducesResponseType(typeof(CertificateDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CertificateDto>> GenerateCertificate([FromBody] GenerateCertificateDto generateDto)
        {
            try
            {
                string userId = GetCurrentUserId();
                _logger.LogInformation("Request to generate certificate for user {UserId} for course {CourseId}", userId, generateDto.CourseId);

                var certificate = await _certificateService.GenerateCertificateAsync(userId, generateDto.CourseId);

                return CreatedAtAction(nameof(GetCertificateById), new { certificateId = certificate.Id }, certificate);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access trying to generate certificate.");
                return Unauthorized(new { message = "User not authenticated or ID not found." });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Course not found when generating certificate for user {UserId}: {ErrorMessage}", GetCurrentUserId(), ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation when generating certificate for user {UserId}: {ErrorMessage}", GetCurrentUserId(), ex.Message);
                return StatusCode(StatusCodes.Status422UnprocessableEntity, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request for certificate generation for user {UserId}: {ErrorMessage}", GetCurrentUserId(), ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error generating certificate for user {UserId} and course {CourseId}", GetCurrentUserId(), generateDto.CourseId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred while generating the certificate." });
            }
        }
    }
}