using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExcellyGenLMS.Application.DTOs.Learner;
using ExcellyGenLMS.Application.Interfaces.Learner;
using ExcellyGenLMS.Core.Interfaces.Repositories.Learner;
using Microsoft.Extensions.Logging;

namespace ExcellyGenLMS.Application.Services.Learner
{
    public class UserProjectService : IUserProjectService
    {
        private readonly IUserProjectRepository _userProjectRepository;
        private readonly ILogger<UserProjectService> _logger;

        public UserProjectService(
            IUserProjectRepository userProjectRepository,
            ILogger<UserProjectService> logger)
        {
            _userProjectRepository = userProjectRepository;
            _logger = logger;
        }

        public async Task<List<ProjectDto>> GetUserProjectsAsync(string userId)
        {
            _logger.LogInformation("Getting projects for user {UserId}", userId);

            var projects = await _userProjectRepository.GetUserProjectsAsync(userId);

            return projects.Select(project => new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description ?? string.Empty,
                Status = project.Status,
                StartDate = project.StartDate.ToString("yyyy-MM-dd"),
                EndDate = project.EndDate?.ToString("yyyy-MM-dd"),
                Role = project.Role,
                Technologies = project.Technologies
                    .Select(pt => pt.Technology.Name)
                    .ToList()
            }).ToList();
        }

        public async Task<ProjectDto> GetUserProjectByIdAsync(string userId, string projectId)
        {
            _logger.LogInformation("Getting project {ProjectId} for user {UserId}", projectId, userId);

            var project = await _userProjectRepository.GetUserProjectByIdAsync(userId, projectId);

            return new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description ?? string.Empty,
                Status = project.Status,
                StartDate = project.StartDate.ToString("yyyy-MM-dd"),
                EndDate = project.EndDate?.ToString("yyyy-MM-dd"),
                Role = project.Role,
                Technologies = project.Technologies
                    .Select(pt => pt.Technology.Name)
                    .ToList()
            };
        }
    }
}