// Path: ExcellyGenLMS.Infrastructure.Data.Repositories.Learner.UserProjectRepository.cs

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExcellyGenLMS.Core.Entities.Learner;
using ExcellyGenLMS.Core.Entities.Admin;
using ExcellyGenLMS.Core.Interfaces.Repositories.Learner;
using ExcellyGenLMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ExcellyGenLMS.Infrastructure.Data.Repositories.Learner
{
    public class UserProjectRepository : IUserProjectRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserProjectRepository> _logger;

        public UserProjectRepository(
            ApplicationDbContext context,
            ILogger<UserProjectRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Project>> GetUserProjectsAsync(string userId)
        {
            _logger.LogInformation("[Repository] GetUserProjectsAsync: Fetching PM assignments for userId: {UserId}", userId);

            var pmAssignmentsForCurrentUser = await _context.PMEmployeeAssignments
                .AsNoTracking()
                .Include(pa => pa.Project)
                    .ThenInclude(pmp => pmp.Technologies)
                        .ThenInclude(pmpt => pmpt.Technology)
                .Where(pa => pa.EmployeeId == userId && pa.Project.Status != "Deleted")
                .OrderByDescending(pa => pa.Project.StartDate)
                .ToListAsync();

            if (!pmAssignmentsForCurrentUser.Any())
            {
                _logger.LogInformation("[Repository] GetUserProjectsAsync: No PM assignments found for userId: {UserId}", userId);
                return new List<Project>();
            }

            _logger.LogInformation("[Repository] GetUserProjectsAsync: Found {Count} PM assignments for userId: {UserId}. Mapping to Core.Learner.Project.", pmAssignmentsForCurrentUser.Count, userId);

            var resultProjects = new List<Project>();

            foreach (var pmAssignment in pmAssignmentsForCurrentUser)
            {
                string learnerStatus = "Assigned";
                if (pmAssignment.Project.Status?.Equals("Completed", System.StringComparison.OrdinalIgnoreCase) == true)
                {
                    learnerStatus = "Completed";
                }

                var learnerProjectInstance = new Project // Core.Entities.Learner.Project
                {
                    Id = pmAssignment.ProjectId,
                    Name = pmAssignment.Project.Name,
                    Description = pmAssignment.Project.Description,
                    Status = learnerStatus,
                    StartDate = pmAssignment.Project.StartDate,
                    EndDate = pmAssignment.Project.Deadline,
                    Role = pmAssignment.Role,
                    Technologies = pmAssignment.Project.Technologies.Select(pmProjectTech => new ProjectTechnology
                    {
                        ProjectId = pmAssignment.ProjectId,
                        TechnologyId = pmProjectTech.TechnologyId,
                        Technology = new Technology
                        {
                            Id = pmProjectTech.Technology.Id,
                            Name = pmProjectTech.Technology.Name,
                            Status = pmProjectTech.Technology.Status,
                            CreatorType = pmProjectTech.Technology.CreatorType,
                            CreatorId = pmProjectTech.Technology.CreatorId,
                        }
                    }).ToList(),
                    UserProjects = new List<UserProject>() // Initialize as per entity structure
                };
                resultProjects.Add(learnerProjectInstance);
            }
            return resultProjects;
        }

        public async Task<List<string>> GetTeamMemberNamesForProjectAsync(string projectId)
        {
            _logger.LogInformation("[Repository] GetTeamMemberNamesForProjectAsync: Fetching team member names for projectId: {ProjectId}", projectId);
            var teamMemberNames = await _context.PMEmployeeAssignments
                .AsNoTracking()
                .Include(pa => pa.Employee) // User entity linked to EmployeeId to get the name
                .Where(pa => pa.ProjectId == projectId && pa.Employee != null) // Ensure employee is not null
                .Select(pa => pa.Employee!.Name) // Select the employee's name. Non-null asserted due to Where clause.
                .Distinct() // Get unique names
                .ToListAsync();

            _logger.LogInformation("[Repository] GetTeamMemberNamesForProjectAsync: Found {Count} team members for projectId: {ProjectId}", teamMemberNames.Count, projectId);
            return teamMemberNames;
        }

        public async Task<Project> GetUserProjectByIdAsync(string userId, string projectId)
        {
            _logger.LogInformation("[Repository] GetUserProjectByIdAsync: Fetching PM assignment for userId: {UserId}, projectId: {ProjectId}", userId, projectId);

            var pmAssignment = await _context.PMEmployeeAssignments
                .AsNoTracking()
                .Include(pa => pa.Project)
                    .ThenInclude(pmp => pmp.Technologies)
                        .ThenInclude(pmpt => pmpt.Technology)
                .FirstOrDefaultAsync(pa => pa.EmployeeId == userId && pa.ProjectId == projectId && pa.Project.Status != "Deleted");

            if (pmAssignment == null)
            {
                _logger.LogWarning("[Repository] GetUserProjectByIdAsync: No PM assignment found for userId: {UserId}, projectId: {ProjectId}", userId, projectId);
                throw new KeyNotFoundException($"Project with ID {projectId} not found or not assigned to user with ID {userId}.");
            }
            _logger.LogInformation("[Repository] GetUserProjectByIdAsync: Found PM assignment. Mapping to Core.Learner.Project.");

            string learnerStatus = "Assigned";
            if (pmAssignment.Project.Status?.Equals("Completed", System.StringComparison.OrdinalIgnoreCase) == true)
            {
                learnerStatus = "Completed";
            }

            // Note: Team members are not fetched here. The service GetUserProjectByIdAsync will call GetTeamMemberNamesForProjectAsync.
            return new Project
            {
                Id = pmAssignment.ProjectId,
                Name = pmAssignment.Project.Name,
                Description = pmAssignment.Project.Description,
                Status = learnerStatus,
                StartDate = pmAssignment.Project.StartDate,
                EndDate = pmAssignment.Project.Deadline,
                Role = pmAssignment.Role,
                Technologies = pmAssignment.Project.Technologies.Select(pmProjectTech => new ProjectTechnology
                {
                    ProjectId = pmAssignment.ProjectId,
                    TechnologyId = pmProjectTech.TechnologyId,
                    Technology = new Technology
                    {
                        Id = pmProjectTech.Technology.Id,
                        Name = pmProjectTech.Technology.Name,
                        Status = pmProjectTech.Technology.Status,
                        CreatorType = pmProjectTech.Technology.CreatorType,
                        CreatorId = pmProjectTech.Technology.CreatorId,
                    }
                }).ToList(),
                UserProjects = new List<UserProject>()
            };
        }
    }
}