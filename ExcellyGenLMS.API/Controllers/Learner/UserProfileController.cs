using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExcellyGenLMS.Application.DTOs.Learner;
using ExcellyGenLMS.Application.Interfaces.Learner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ExcellyGenLMS.API.Controllers.Learner
{
    [Route("api/user-profile")]
    [ApiController]
    [Authorize]
    public class UserProfileController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;
        private readonly ILogger<UserProfileController> _logger;

        public UserProfileController(
            IUserProfileService userProfileService,
            ILogger<UserProfileController> logger)
        {
            _userProfileService = userProfileService;
            _logger = logger;
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<UserProfileDto>> GetUserProfile(string userId)
        {
            try
            {
                _logger.LogInformation("Fetching profile for user {UserId}", userId);
                var profile = await _userProfileService.GetUserProfileAsync(userId);
                return Ok(profile);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found: {UserId}", userId);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile: {UserId}", userId);
                return StatusCode(500, new { error = "Failed to retrieve user profile." });
            }
        }

        [HttpPut("{userId}")]
        public async Task<ActionResult<UserProfileDto>> UpdateUserProfile(
            string userId,
            [FromBody] UpdateUserProfileDto updateDto)
        {
            try
            {
                _logger.LogInformation("Updating profile for user {UserId}", userId);
                var updatedProfile = await _userProfileService.UpdateUserProfileAsync(userId, updateDto);
                return Ok(updatedProfile);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found: {UserId}", userId);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile: {UserId}", userId);
                return StatusCode(500, new { error = "Failed to update user profile." });
            }
        }

        [HttpPost("{userId}/avatar")]
        public async Task<ActionResult<object>> UploadUserAvatar(string userId, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { error = "No file uploaded." });
                }

                // Check file size (5MB max)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new { error = "File size exceeds 5MB limit." });
                }

                // Check file type
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/jpg" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                {
                    return BadRequest(new { error = "File type not allowed. Only JPEG, PNG, and GIF files are accepted." });
                }

                _logger.LogInformation("Uploading avatar for user {UserId}", userId);

                // Use the existing service method which now supports Firebase
                var avatarUrl = await _userProfileService.UploadUserAvatarAsync(userId, file);

                return Ok(new { avatar = avatarUrl });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found: {UserId}", userId);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading avatar: {UserId}", userId);
                return StatusCode(500, new { error = "Failed to upload avatar." });
            }
        }

        [HttpPost("{userId}/avatar-url")]
        public async Task<ActionResult<object>> UpdateAvatarUrl(
            string userId,
            [FromBody] AvatarUrlDto avatarUrlDto)
        {
            try
            {
                if (string.IsNullOrEmpty(avatarUrlDto.AvatarUrl))
                {
                    return BadRequest(new { error = "Avatar URL is required." });
                }

                _logger.LogInformation("Updating avatar URL for user {UserId}", userId);
                var avatarUrl = await _userProfileService.UpdateAvatarUrlAsync(userId, avatarUrlDto.AvatarUrl);
                return Ok(new { avatar = avatarUrl });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found: {UserId}", userId);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating avatar URL: {UserId}", userId);
                return StatusCode(500, new { error = "Failed to update avatar URL." });
            }
        }

        [HttpDelete("{userId}/avatar")]
        public async Task<ActionResult<object>> DeleteUserAvatar(string userId)
        {
            try
            {
                _logger.LogInformation("Deleting avatar for user {UserId}", userId);

                // Get current profile to return previous avatar URL
                var currentProfile = await _userProfileService.GetUserProfileAsync(userId);
                var previousAvatarUrl = currentProfile.Avatar;

                // Delete the avatar
                await _userProfileService.DeleteUserAvatarAsync(userId);

                return Ok(new { previousAvatarUrl = previousAvatarUrl });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found: {UserId}", userId);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting avatar: {UserId}", userId);
                return StatusCode(500, new { error = "Failed to delete avatar." });
            }
        }
    }

    /// <summary>
    /// Data transfer object for avatar URL updates
    /// </summary>
    public class AvatarUrlDto
    {
        public string AvatarUrl { get; set; } = string.Empty;
    }
}