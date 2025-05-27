// ExcellyGenLMS.Application/Services/Course/CertificateService.cs
using ExcellyGenLMS.Application.DTOs.Course;
using ExcellyGenLMS.Application.Interfaces.Course;
using ExcellyGenLMS.Core.Entities.Course;
using ExcellyGenLMS.Core.Interfaces.Infrastructure;
using ExcellyGenLMS.Core.Interfaces.Repositories.Course;
using ExcellyGenLMS.Core.Interfaces.Repositories.Auth; // ADDED: For IUserRepository
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Services.Course
{
    public class CertificateService : ICertificateService
    {
        private readonly ICertificateRepository _certificateRepository;
        private readonly ILearnerCourseService _learnerCourseService;
        private readonly ICourseRepository _courseRepository;
        private readonly IUserRepository _userRepository; // ADDED: User repository
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<CertificateService> _logger;

        public CertificateService(
            ICertificateRepository certificateRepository,
            ILearnerCourseService learnerCourseService,
            ICourseRepository courseRepository,
            IUserRepository userRepository, // ADDED: Inject IUserRepository
            IFileStorageService fileStorageService,
            ILogger<CertificateService> logger)
        {
            _certificateRepository = certificateRepository;
            _learnerCourseService = learnerCourseService;
            _courseRepository = courseRepository;
            _userRepository = userRepository; // ASSIGN: Assign userRepository
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        private CertificateDto? MapCertificateToDto(Certificate? certificate)
        {
            if (certificate == null) return null;
            return new CertificateDto
            {
                Id = certificate.Id,
                UserId = certificate.UserId,
                UserName = certificate.User?.Name ?? "Unknown User",
                CourseId = certificate.CourseId,
                CourseTitle = certificate.Course?.Title ?? "Unknown Course",
                CompletionDate = certificate.CompletionDate,
                Title = certificate.Title,
                CertificateFileUrl = !string.IsNullOrEmpty(certificate.FilePath) ? _fileStorageService.GetFileUrl(certificate.FilePath) : string.Empty
            };
        }

        public async Task<CertificateDto?> GetCertificateByIdAsync(int certificateId)
        {
            _logger.LogInformation("Attempting to retrieve certificate {CertificateId}", certificateId);
            var certificate = await _certificateRepository.GetByIdAsync(certificateId);
            return MapCertificateToDto(certificate);
        }

        public async Task<IEnumerable<CertificateDto>> GetCertificatesByUserIdAsync(string userId)
        {
            _logger.LogInformation("Retrieving certificates for user {UserId}", userId);
            var certificates = await _certificateRepository.GetCertificatesByUserIdAsync(userId);
            return certificates.Select(MapCertificateToDto).Where(c => c != null).ToList()!;
        }

        public async Task<CertificateDto> GenerateCertificateAsync(string userId, int courseId)
        {
            _logger.LogInformation("Attempting to generate certificate for user {UserId} and course {CourseId}", userId, courseId);

            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
            {
                throw new KeyNotFoundException($"Course with ID {courseId} not found.");
            }

            var learnerUser = await _userRepository.GetUserByIdAsync(userId); // FETCH LEARNER USER
            if (learnerUser == null)
            {
                throw new KeyNotFoundException($"Learner user with ID {userId} not found.");
            }

            var existingCertificate = await _certificateRepository.GetCertificateByUserIdAndCourseIdAsync(userId, courseId);
            if (existingCertificate != null)
            {
                _logger.LogInformation("Certificate already exists for user {UserId} and course {CourseId}. Returning existing.", userId, courseId);
                return MapCertificateToDto(existingCertificate)!;
            }

            bool courseCompleted = await _learnerCourseService.HasLearnerCompletedAllCourseContentAsync(userId, courseId);
            if (!courseCompleted)
            {
                throw new InvalidOperationException($"User {learnerUser.Name} has not completed all required content for course {course.Title}.");
            }

            // Generate certificate content using learnerUser.Name
            string certificateContent = $"Certificate of Completion\n\nThis certifies that {learnerUser.Name} has successfully completed the course:\n\"{course.Title}\"\n\nAwarded on: {DateTime.UtcNow.ToShortDateString()}\n\nExcellyGen LMS";
            byte[] certificateData = System.Text.Encoding.UTF8.GetBytes(certificateContent);

            string fileName = $"certificate_{userId}_{courseId}.pdf";
            string containerName = "certificates";
            string? filePathInStorage = null;

            using (var stream = new MemoryStream(certificateData))
            {
                filePathInStorage = await _fileStorageService.SaveFileAsync(stream, fileName, "application/pdf", containerName);
            }

            if (string.IsNullOrEmpty(filePathInStorage))
            {
                throw new InvalidOperationException("Failed to save the generated certificate file to storage.");
            }
            _logger.LogInformation("Dummy certificate saved to storage at: {FilePath}", filePathInStorage);

            var newCertificate = new Certificate
            {
                UserId = userId,
                CourseId = courseId,
                Title = $"Certificate: {course.Title}",
                CompletionDate = DateTime.UtcNow,
                CertificateData = certificateData,
                FilePath = filePathInStorage
            };

            var createdCertificate = await _certificateRepository.AddAsync(newCertificate);
            _logger.LogInformation("Certificate {CertificateId} generated and recorded for user {UserId} and course {CourseId}.", createdCertificate.Id, userId, courseId);

            return MapCertificateToDto(createdCertificate)!;
        }
    }
}