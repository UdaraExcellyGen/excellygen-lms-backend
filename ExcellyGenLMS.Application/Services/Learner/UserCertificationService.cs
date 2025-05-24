using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExcellyGenLMS.Application.DTOs.Learner;
using ExcellyGenLMS.Application.Interfaces.Common;
using ExcellyGenLMS.Application.Interfaces.Learner;
using ExcellyGenLMS.Core.Interfaces.Repositories.Learner;
using Microsoft.Extensions.Logging;

namespace ExcellyGenLMS.Application.Services.Learner
{
    public class UserCertificationService : IUserCertificationService
    {
        private readonly IUserCertificationRepository _userCertificationRepository;
        private readonly IFileService _fileService;
        private readonly ILogger<UserCertificationService> _logger;

        public UserCertificationService(
            IUserCertificationRepository userCertificationRepository,
            IFileService fileService,
            ILogger<UserCertificationService> logger)
        {
            _userCertificationRepository = userCertificationRepository;
            _fileService = fileService;
            _logger = logger;
        }

        public async Task<List<CertificationDto>> GetUserCertificationsAsync(string userId)
        {
            _logger.LogInformation("Getting certifications for user {UserId}", userId);

            var userCertifications = await _userCertificationRepository.GetUserCertificationsAsync(userId);

            return userCertifications.Select(uc => new CertificationDto
            {
                Id = uc.Certification.Id,
                Name = uc.Certification.Name,
                IssuingOrganization = uc.Certification.IssuingOrganization ?? string.Empty,
                Description = uc.Certification.Description ?? string.Empty,
                IssueDate = uc.IssueDate.ToString("MMM yyyy"),
                Status = uc.Status,
                CredentialId = uc.Certification.CredentialId ?? string.Empty,
                ImageUrl = _fileService.GetFullImageUrl(uc.Certification.ImagePath)
            }).ToList();
        }

        public async Task<CertificationDto> GetUserCertificationByIdAsync(string userId, string certificationId)
        {
            _logger.LogInformation("Getting certification {CertificationId} for user {UserId}", certificationId, userId);

            var userCertification = await _userCertificationRepository.GetUserCertificationByIdAsync(userId, certificationId);

            return new CertificationDto
            {
                Id = userCertification.Certification.Id,
                Name = userCertification.Certification.Name,
                IssuingOrganization = userCertification.Certification.IssuingOrganization ?? string.Empty,
                Description = userCertification.Certification.Description ?? string.Empty,
                IssueDate = userCertification.IssueDate.ToString("MMM yyyy"),
                Status = userCertification.Status,
                CredentialId = userCertification.Certification.CredentialId ?? string.Empty,
                ImageUrl = _fileService.GetFullImageUrl(userCertification.Certification.ImagePath)
            };
        }
    }
}