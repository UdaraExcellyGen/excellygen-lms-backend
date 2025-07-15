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
    [Authorize] // Changed: Authorize all actions, apply specific roles where needed
    public class ExternalCertificatesController : ControllerBase
    {
        private readonly IExternalCertificateService _externalCertificateService;
        private readonly ILogger<ExternalCertificatesController> _logger;

        public ExternalCertificatesController(
            IExternalCertificateService externalCertificateService,
            ILogger<ExternalCertificatesController> logger)
        {
            _externalCertificateService = externalCertificateService ?? throw new ArgumentNullException(nameof(externalCertificateService));
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

        // GET: api/ExternalCertificates/user (For logged-in user's own page)
        [HttpGet("user")]
        [Authorize(Roles = "Learner")]
        public async Task<ActionResult<IEnumerable<ExternalCertificateDto>>> GetUserExternalCertificates()
        {
            // ... (Existing code for this method remains unchanged)
            try
            {
                string userId = GetCurrentUserId();
                var certificates = await _externalCertificateService.GetByUserIdAsync(userId);
                return Ok(certificates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving external certificates for current user.");
                return StatusCode(500, "An error occurred.");
            }
        }

        // ========================================================================
        // NEW METHOD: GET: api/ExternalCertificates/user/{userId} (For public profiles)
        // ========================================================================
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<ExternalCertificateDto>>> GetUserExternalCertificatesByUserId(string userId)
        {
            try
            {
                _logger.LogInformation("Fetching external certificates for profile view of user {UserId}", userId);
                var certificates = await _externalCertificateService.GetByUserIdAsync(userId);
                return Ok(certificates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving external certificates for user {UserId}", userId);
                return StatusCode(500, "An error occurred while retrieving external certificates.");
            }
        }
        // ========================================================================

        // The rest of the methods remain unchanged...

        [HttpGet("{certificateId}")]
        [Authorize(Roles = "Learner")]
        public async Task<ActionResult<ExternalCertificateDto>> GetExternalCertificateById(string certificateId)
        {
            // ... (Existing code)
            string userId = GetCurrentUserId();
            var certificate = await _externalCertificateService.GetByIdAsync(certificateId);
            if (certificate == null) return NotFound();
            if (certificate.UserId != userId) return Forbid();
            return Ok(certificate);
        }

        [HttpPost]
        [Authorize(Roles = "Learner")]
        public async Task<ActionResult<ExternalCertificateDto>> AddExternalCertificate([FromBody] AddExternalCertificateDto addDto)
        {
            // ... (Existing code)
            string userId = GetCurrentUserId();
            var certificate = await _externalCertificateService.AddAsync(userId, addDto);
            return CreatedAtAction(nameof(GetExternalCertificateById), new { certificateId = certificate.Id }, certificate);
        }

        [HttpPut("{certificateId}")]
        [Authorize(Roles = "Learner")]
        public async Task<ActionResult<ExternalCertificateDto>> UpdateExternalCertificate(string certificateId, [FromBody] UpdateExternalCertificateDto updateDto)
        {
            // ... (Existing code)
            string userId = GetCurrentUserId();
            var certificate = await _externalCertificateService.UpdateAsync(userId, certificateId, updateDto);
            return Ok(certificate);
        }

        [HttpDelete("{certificateId}")]
        [Authorize(Roles = "Learner")]
        public async Task<ActionResult> DeleteExternalCertificate(string certificateId)
        {
            // ... (Existing code)
            string userId = GetCurrentUserId();
            await _externalCertificateService.DeleteAsync(userId, certificateId);
            return NoContent();
        }
    }
}