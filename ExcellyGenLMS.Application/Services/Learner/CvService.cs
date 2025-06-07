using System.Linq;
using System.Threading.Tasks;
using ExcellyGenLMS.Application.DTOs.Learner;      // <<<< ADJUSTED using for DTOs new location
using ExcellyGenLMS.Application.Interfaces.Learner;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace ExcellyGenLMS.Application.Services.Learner
{
    public class CvService : ICvService
    {
        private readonly IUserProfileService _userProfileService;
        private readonly IUserProjectService _userProjectService;
        private readonly IUserCertificationService _userCertificationService;
        private readonly IUserTechnologyService _userTechnologyService;
        private readonly ILogger<CvService> _logger;

        public CvService(
            IUserProfileService userProfileService,
            IUserProjectService userProjectService,
            IUserCertificationService userCertificationService,
            IUserTechnologyService userTechnologyService,
            ILogger<CvService> logger)
        {
            _userProfileService = userProfileService;
            _userProjectService = userProjectService;
            _userCertificationService = userCertificationService;
            _userTechnologyService = userTechnologyService;
            _logger = logger;
        }

        public async Task<CvDto> GetCvDataAsync(string userId)
        {
            _logger.LogInformation("Fetching CV data for user {UserId}", userId);

            var userProfile = await _userProfileService.GetUserProfileAsync(userId);
            if (userProfile == null)
            {
                _logger.LogWarning("User profile not found for CV generation. UserId: {UserId}", userId);
                throw new KeyNotFoundException($"User profile with ID {userId} not found.");
            }

            var userProjects = await _userProjectService.GetUserProjectsAsync(userId);
            var userCertifications = await _userCertificationService.GetUserCertificationsAsync(userId);
            var userTechnologies = await _userTechnologyService.GetUserTechnologiesAsync(userId);

            var cvDto = new CvDto
            {
                PersonalInfo = new CvPersonalInfo
                {
                    Name = userProfile.Name,
                    Position = userProfile.JobRole ?? "N/A",
                    Email = userProfile.Email,
                    Phone = userProfile.Phone,
                    Department = userProfile.Department,
                    Photo = userProfile.Avatar,
                    Summary = userProfile.About ?? string.Empty
                },
                Projects = userProjects
                    .Where(p => p.Status?.ToLower() == "completed")
                    .Select(p => new CvProjectDto
                    {
                        Title = p.Name,
                        Description = p.Description,
                        Technologies = p.Technologies ?? new List<string>(),
                        StartDate = p.StartDate,
                        CompletionDate = p.EndDate,
                        Status = p.Status
                    }).ToList(),
                Courses = userCertifications
                    .Where(c => c.Status?.ToLower() == "completed")
                    .Select(c => new CvCourseDto
                    {
                        Title = c.Name,
                        Provider = c.IssuingOrganization ?? "N/A",
                        CompletionDate = c.IssueDate,
                        Duration = null,
                        Certificate = true
                    }).ToList(),
                Skills = userTechnologies.Select(t => t.Name).ToList()
            };

            _logger.LogInformation("Successfully fetched CV data for user {UserId}", userId);
            return cvDto;
        }
    }
}