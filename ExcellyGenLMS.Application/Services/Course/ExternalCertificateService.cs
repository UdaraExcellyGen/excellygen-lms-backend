// ExcellyGenLMS.Application/Services/Course/ExternalCertificateService.cs
using ExcellyGenLMS.Application.DTOs.Course;
using ExcellyGenLMS.Application.Interfaces.Course;
using ExcellyGenLMS.Core.Entities.Course;
using ExcellyGenLMS.Core.Interfaces.Repositories.Course;
using ExcellyGenLMS.Core.Interfaces.Repositories.Auth;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Services.Course
{
    public class ExternalCertificateService : IExternalCertificateService
    {
        private readonly IExternalCertificateRepository _externalCertificateRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<ExternalCertificateService> _logger;

        public ExternalCertificateService(
            IExternalCertificateRepository externalCertificateRepository,
            IUserRepository userRepository,
            ILogger<ExternalCertificateService> logger)
        {
            _externalCertificateRepository = externalCertificateRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        private ExternalCertificateDto? MapToDto(ExternalCertificate? externalCertificate)
        {
            if (externalCertificate == null) return null;

            return new ExternalCertificateDto
            {
                Id = externalCertificate.Id,
                UserId = externalCertificate.UserId,
                UserName = externalCertificate.User?.Name ?? "Unknown User",
                Title = externalCertificate.Title,
                Issuer = externalCertificate.Issuer,
                Platform = externalCertificate.Platform,
                CompletionDate = externalCertificate.CompletionDate,
                CredentialUrl = externalCertificate.CredentialUrl,
                CredentialId = externalCertificate.CredentialId,
                Description = externalCertificate.Description,
                ImageUrl = externalCertificate.ImageUrl,
                CreatedAt = externalCertificate.CreatedAt,
                UpdatedAt = externalCertificate.UpdatedAt
            };
        }

        public async Task<ExternalCertificateDto?> GetByIdAsync(string id)
        {
            _logger.LogInformation("Retrieving external certificate {CertificateId}", id);
            var externalCertificate = await _externalCertificateRepository.GetByIdAsync(id);
            return MapToDto(externalCertificate);
        }

        public async Task<IEnumerable<ExternalCertificateDto>> GetByUserIdAsync(string userId)
        {
            _logger.LogInformation("Retrieving external certificates for user {UserId}", userId);
            var externalCertificates = await _externalCertificateRepository.GetByUserIdAsync(userId);
            return externalCertificates.Select(MapToDto).Where(dto => dto != null).ToList()!;
        }

        public async Task<ExternalCertificateDto> AddAsync(string userId, AddExternalCertificateDto addDto)
        {
            _logger.LogInformation("Adding external certificate for user {UserId}: {Title}", userId, addDto.Title);

            // Validate user exists
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            // Validate completion date is not in the future
            if (addDto.CompletionDate > DateTime.UtcNow)
            {
                throw new ArgumentException("Completion date cannot be in the future.");
            }

            var externalCertificate = new ExternalCertificate
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Title = addDto.Title.Trim(),
                Issuer = addDto.Issuer.Trim(),
                Platform = addDto.Platform.Trim(),
                CompletionDate = addDto.CompletionDate,
                CredentialUrl = string.IsNullOrWhiteSpace(addDto.CredentialUrl) ? null : addDto.CredentialUrl.Trim(),
                CredentialId = string.IsNullOrWhiteSpace(addDto.CredentialId) ? null : addDto.CredentialId.Trim(),
                Description = string.IsNullOrWhiteSpace(addDto.Description) ? null : addDto.Description.Trim()
            };

            var createdCertificate = await _externalCertificateRepository.AddAsync(externalCertificate);
            _logger.LogInformation("External certificate {CertificateId} created for user {UserId}", createdCertificate.Id, userId);

            return MapToDto(createdCertificate)!;
        }

        public async Task<ExternalCertificateDto> UpdateAsync(string userId, string certificateId, UpdateExternalCertificateDto updateDto)
        {
            _logger.LogInformation("Updating external certificate {CertificateId} for user {UserId}", certificateId, userId);

            // Check if certificate exists and belongs to user
            if (!await UserOwnsExternalCertificateAsync(userId, certificateId))
            {
                throw new UnauthorizedAccessException($"User {userId} does not own external certificate {certificateId} or certificate does not exist.");
            }

            var existingCertificate = await _externalCertificateRepository.GetByIdAsync(certificateId);
            if (existingCertificate == null)
            {
                throw new KeyNotFoundException($"External certificate with ID {certificateId} not found.");
            }

            // Update only provided fields
            if (!string.IsNullOrWhiteSpace(updateDto.Title))
                existingCertificate.Title = updateDto.Title.Trim();

            if (!string.IsNullOrWhiteSpace(updateDto.Issuer))
                existingCertificate.Issuer = updateDto.Issuer.Trim();

            if (!string.IsNullOrWhiteSpace(updateDto.Platform))
                existingCertificate.Platform = updateDto.Platform.Trim();

            if (updateDto.CompletionDate.HasValue)
            {
                if (updateDto.CompletionDate.Value > DateTime.UtcNow)
                {
                    throw new ArgumentException("Completion date cannot be in the future.");
                }
                existingCertificate.CompletionDate = updateDto.CompletionDate.Value;
            }

            if (updateDto.CredentialUrl != null)
                existingCertificate.CredentialUrl = string.IsNullOrWhiteSpace(updateDto.CredentialUrl) ? null : updateDto.CredentialUrl.Trim();

            if (updateDto.CredentialId != null)
                existingCertificate.CredentialId = string.IsNullOrWhiteSpace(updateDto.CredentialId) ? null : updateDto.CredentialId.Trim();

            if (updateDto.Description != null)
                existingCertificate.Description = string.IsNullOrWhiteSpace(updateDto.Description) ? null : updateDto.Description.Trim();

            var updatedCertificate = await _externalCertificateRepository.UpdateAsync(existingCertificate);
            _logger.LogInformation("External certificate {CertificateId} updated for user {UserId}", certificateId, userId);

            return MapToDto(updatedCertificate)!;
        }

        public async Task<bool> DeleteAsync(string userId, string certificateId)
        {
            _logger.LogInformation("Deleting external certificate {CertificateId} for user {UserId}", certificateId, userId);

            // Check if certificate exists and belongs to user
            if (!await UserOwnsExternalCertificateAsync(userId, certificateId))
            {
                throw new UnauthorizedAccessException($"User {userId} does not own external certificate {certificateId} or certificate does not exist.");
            }

            var result = await _externalCertificateRepository.DeleteAsync(certificateId);
            if (result)
            {
                _logger.LogInformation("External certificate {CertificateId} deleted for user {UserId}", certificateId, userId);
            }
            else
            {
                _logger.LogWarning("Failed to delete external certificate {CertificateId} for user {UserId}", certificateId, userId);
            }

            return result;
        }

        public async Task<bool> UserOwnsExternalCertificateAsync(string userId, string certificateId)
        {
            return await _externalCertificateRepository.UserOwnsExternalCertificateAsync(userId, certificateId);
        }
    }
}