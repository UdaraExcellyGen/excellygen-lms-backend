// ExcellyGenLMS.API/Controllers/Course/ExternalCertificatesController.cs
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

        // GET: api/ExternalCertificates/user
        [HttpGet("user")]
        [ProducesResponseType(typeof(IEnumerable<ExternalCertificateDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ExternalCertificateDto>>> GetUserExternalCertificates()
        {
            try
            {
                string userId = GetCurrentUserId();
                _logger.LogInformation("Retrieving external certificates for user {UserId}", userId);
                var certificates = await _externalCertificateService.GetByUserIdAsync(userId);
                return Ok(certificates);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access trying to get user external certificates.");
                return Unauthorized(new { message = "User not authenticated or ID not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving external certificates for user {UserId}", GetCurrentUserId());
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving external certificates." });
            }
        }

        // GET: api/ExternalCertificates/{certificateId}
        [HttpGet("{certificateId}")]
        [ProducesResponseType(typeof(ExternalCertificateDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ExternalCertificateDto>> GetExternalCertificateById(string certificateId)
        {
            try
            {
                string userId = GetCurrentUserId();
                var certificate = await _externalCertificateService.GetByIdAsync(certificateId);

                if (certificate == null)
                {
                    _logger.LogWarning("External certificate {CertificateId} not found.", certificateId);
                    return NotFound(new { message = $"External certificate with ID {certificateId} not found." });
                }

                if (certificate.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} attempted to access external certificate {CertificateId} belonging to user {CertUserId}",
                        userId, certificateId, certificate.UserId);
                    return Forbid();
                }

                return Ok(certificate);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access trying to get external certificate.");
                return Unauthorized(new { message = "User not authenticated or ID not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving external certificate {CertificateId} for user {UserId}", certificateId, GetCurrentUserId());
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving the external certificate." });
            }
        }

        // POST: api/ExternalCertificates
        [HttpPost]
        [ProducesResponseType(typeof(ExternalCertificateDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ExternalCertificateDto>> AddExternalCertificate([FromBody] AddExternalCertificateDto addDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                string userId = GetCurrentUserId();
                _logger.LogInformation("Request to add external certificate for user {UserId}: {Title}", userId, addDto.Title);

                var certificate = await _externalCertificateService.AddAsync(userId, addDto);

                return CreatedAtAction(nameof(GetExternalCertificateById), new { certificateId = certificate.Id }, certificate);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access trying to add external certificate.");
                return Unauthorized(new { message = "User not authenticated or ID not found." });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found when adding external certificate for user {UserId}: {ErrorMessage}", GetCurrentUserId(), ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when adding external certificate for user {UserId}: {ErrorMessage}", GetCurrentUserId(), ex.Message);
                return StatusCode(StatusCodes.Status422UnprocessableEntity, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error adding external certificate for user {UserId}", GetCurrentUserId());
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred while adding the external certificate." });
            }
        }

        // PUT: api/ExternalCertificates/{certificateId}
        [HttpPut("{certificateId}")]
        [ProducesResponseType(typeof(ExternalCertificateDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ExternalCertificateDto>> UpdateExternalCertificate(string certificateId, [FromBody] UpdateExternalCertificateDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                string userId = GetCurrentUserId();
                _logger.LogInformation("Request to update external certificate {CertificateId} for user {UserId}", certificateId, userId);

                var certificate = await _externalCertificateService.UpdateAsync(userId, certificateId, updateDto);

                return Ok(certificate);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access trying to update external certificate {CertificateId}.", certificateId);
                if (ex.Message.Contains("does not own"))
                {
                    return Forbid();
                }
                return Unauthorized(new { message = "User not authenticated or ID not found." });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "External certificate {CertificateId} not found for user {UserId}: {ErrorMessage}", certificateId, GetCurrentUserId(), ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when updating external certificate {CertificateId} for user {UserId}: {ErrorMessage}", certificateId, GetCurrentUserId(), ex.Message);
                return StatusCode(StatusCodes.Status422UnprocessableEntity, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating external certificate {CertificateId} for user {UserId}", certificateId, GetCurrentUserId());
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred while updating the external certificate." });
            }
        }

        // DELETE: api/ExternalCertificates/{certificateId}
        [HttpDelete("{certificateId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteExternalCertificate(string certificateId)
        {
            try
            {
                string userId = GetCurrentUserId();
                _logger.LogInformation("Request to delete external certificate {CertificateId} for user {UserId}", certificateId, userId);

                var result = await _externalCertificateService.DeleteAsync(userId, certificateId);

                if (result)
                {
                    return NoContent();
                }
                else
                {
                    return NotFound(new { message = $"External certificate with ID {certificateId} not found." });
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access trying to delete external certificate {CertificateId}.", certificateId);
                if (ex.Message.Contains("does not own"))
                {
                    return Forbid();
                }
                return Unauthorized(new { message = "User not authenticated or ID not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting external certificate {CertificateId} for user {UserId}", certificateId, GetCurrentUserId());
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred while deleting the external certificate." });
            }
        }
    }
}