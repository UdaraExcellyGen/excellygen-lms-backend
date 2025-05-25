// Path: ExcellyGenLMS.Application.Services.Learner.UserProjectService.cs

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
            _logger.LogInformation("[Service] GetUserProjectsAsync: Calling repository to get core project entities for userId: {UserId}", userId);
            var coreLearnerProjects = await _userProjectRepository.GetUserProjectsAsync(userId);

            if (!coreLearnerProjects.Any())
            {
                _logger.LogInformation("[Service] GetUserProjectsAsync: No core project entities returned for userId: {UserId}", userId);
                return new List<ProjectDto>();
            }

            _logger.LogInformation("[Service] GetUserProjectsAsync: Mapping {Count} core projects to DTOs and fetching team members for userId: {UserId}", coreLearnerProjects.Count, userId);

            var projectDtos = new List<ProjectDto>();
            foreach (var projectEntity in coreLearnerProjects)
            {
                // Fetch team members for each project using the projectEntity.Id (which is PMProject.Id)
                _logger.LogDebug("[Service] GetUserProjectsAsync: Fetching team for projectId: {ProjectId}", projectEntity.Id);
                var teamMemberNames = await _userProjectRepository.GetTeamMemberNamesForProjectAsync(projectEntity.Id);

                projectDtos.Add(new ProjectDto
                {
                    Id = projectEntity.Id,
                    Name = projectEntity.Name,
                    Description = projectEntity.Description ?? string.Empty,
                    Status = projectEntity.Status,
                    StartDate = projectEntity.StartDate.ToString("yyyy-MM-dd"),
                    EndDate = projectEntity.EndDate?.ToString("yyyy-MM-dd"),
                    Role = projectEntity.Role,
                    Technologies = projectEntity.Technologies?
                        .Select(pt => pt.Technology?.Name ?? "N/A")
                        .Where(name => name != "N/A")
                        .ToList() ?? new List<string>(),
                    Team = teamMemberNames // Assign the fetched team members to the DTO
                });
            }
            return projectDtos;
        }

        public async Task<ProjectDto> GetUserProjectByIdAsync(string userId, string projectId)
        {
            _logger.LogInformation("[Service] GetUserProjectByIdAsync: Calling repository for core project entity. UserId: {UserId}, ProjectId: {ProjectId}", userId, projectId);
            var projectEntity = await _userProjectRepository.GetUserProjectByIdAsync(userId, projectId);

            _logger.LogInformation("[Service] GetUserProjectByIdAsync: Fetching team for projectId: {ProjectId}", projectEntity.Id);
            var teamMemberNames = await _userProjectRepository.GetTeamMemberNamesForProjectAsync(projectEntity.Id);

            _logger.LogInformation("[Service] GetUserProjectByIdAsync: Mapping core project entity to DTO.");
            return new ProjectDto
            {
                Id = projectEntity.Id,
                Name = projectEntity.Name,
                Description = projectEntity.Description ?? string.Empty,
                Status = projectEntity.Status,
                StartDate = projectEntity.StartDate.ToString("yyyy-MM-dd"),
                EndDate = projectEntity.EndDate?.ToString("yyyy-MM-dd"),
                Role = projectEntity.Role,
                Technologies = projectEntity.Technologies?
                    .Select(pt => pt.Technology?.Name ?? "N/A")
                    .Where(name => name != "N/A")
                    .ToList() ?? new List<string>(),
                Team = teamMemberNames // Assign the fetched team members
            };
        }
    }
}