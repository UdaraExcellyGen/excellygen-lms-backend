using System;
using System.IO;
using System.Threading.Tasks;
using ExcellyGenLMS.Application.DTOs.Learner;
using ExcellyGenLMS.Application.Interfaces.Common;
using ExcellyGenLMS.Application.Interfaces.Learner;
using ExcellyGenLMS.Core.Entities.Auth;
using ExcellyGenLMS.Core.Interfaces.Repositories.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ExcellyGenLMS.Application.Services.Learner
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IUserRepository _userRepository;
        private readonly IFileService _fileService;
        private readonly ILogger<UserProfileService> _logger;

        public UserProfileService(
            IUserRepository userRepository,
            IFileService fileService,
            ILogger<UserProfileService> logger)
        {
            _userRepository = userRepository;
            _fileService = fileService;
            _logger = logger;
        }

        public async Task<UserProfileDto> GetUserProfileAsync(string userId)
        {
            _logger.LogInformation("Getting profile for user {UserId}", userId);

            var user = await _userRepository.GetUserByIdAsync(userId)
                ?? throw new KeyNotFoundException($"User with ID {userId} not found");

            return MapUserToProfileDto(user);
        }

        public async Task<UserProfileDto> UpdateUserProfileAsync(string userId, UpdateUserProfileDto updateDto)
        {
            _logger.LogInformation("Updating profile for user {UserId}", userId);

            var user = await _userRepository.GetUserByIdAsync(userId)
                ?? throw new KeyNotFoundException($"User with ID {userId} not found");

            // Update properties only if they're provided in the DTO
            if (!string.IsNullOrEmpty(updateDto.JobRole))
            {
                user.JobRole = updateDto.JobRole;
            }

            if (!string.IsNullOrEmpty(updateDto.About))
            {
                user.About = updateDto.About;
            }

            await _userRepository.UpdateUserAsync(user);

            _logger.LogInformation("Profile updated for user {UserId}", userId);

            return MapUserToProfileDto(user);
        }

        public async Task<string> UploadUserAvatarAsync(string userId, IFormFile avatar)
        {
            _logger.LogInformation("Uploading avatar for user {UserId}", userId);

            var user = await _userRepository.GetUserByIdAsync(userId)
                ?? throw new KeyNotFoundException($"User with ID {userId} not found");

            // Delete existing avatar if there is one
            if (!string.IsNullOrEmpty(user.Avatar))
            {
                await _fileService.DeleteFileAsync(user.Avatar);
            }

            // Generate a unique file name
            var fileName = $"{userId}_{Guid.NewGuid()}{Path.GetExtension(avatar.FileName)}";

            // Save the file
            var avatarUrl = await _fileService.SaveFileAsync(avatar, "avatars");

            // Update user avatar URL
            user.Avatar = avatarUrl;
            await _userRepository.UpdateUserAsync(user);

            _logger.LogInformation("Avatar uploaded for user {UserId}", userId);

            // Return full URL
            return _fileService.GetFullImageUrl(avatarUrl);
        }

        public async Task DeleteUserAvatarAsync(string userId)
        {
            _logger.LogInformation("Deleting avatar for user {UserId}", userId);

            var user = await _userRepository.GetUserByIdAsync(userId)
                ?? throw new KeyNotFoundException($"User with ID {userId} not found");

            if (!string.IsNullOrEmpty(user.Avatar))
            {
                await _fileService.DeleteFileAsync(user.Avatar);
                user.Avatar = null;
                await _userRepository.UpdateUserAsync(user);

                _logger.LogInformation("Avatar deleted for user {UserId}", userId);
            }
            else
            {
                _logger.LogInformation("No avatar found to delete for user {UserId}", userId);
            }
        }

        private UserProfileDto MapUserToProfileDto(User user)
        {
            string? avatarUrl = null;

            if (!string.IsNullOrEmpty(user.Avatar))
            {
                avatarUrl = _fileService.GetFullImageUrl(user.Avatar);
            }

            return new UserProfileDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Department = user.Department,
                JobRole = user.JobRole,
                About = user.About,
                Avatar = avatarUrl,
                Roles = user.Roles
            };
        }
    }
}