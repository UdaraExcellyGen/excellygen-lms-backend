// Path: ExcellyGenLMS.Application/Services/ProjectManager/EmployeeAssignmentService.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ExcellyGenLMS.Application.DTOs.ProjectManager;
using ExcellyGenLMS.Application.Interfaces.ProjectManager;
using ExcellyGenLMS.Core.Entities.ProjectManager;
using ExcellyGenLMS.Core.Interfaces.Repositories.ProjectManager;
using ExcellyGenLMS.Core.Interfaces.Repositories.Auth;
using ExcellyGenLMS.Core.Interfaces.Repositories.Learner;

namespace ExcellyGenLMS.Application.Services.ProjectManager
{
    public class EmployeeAssignmentService : IEmployeeAssignmentService
    {
        private readonly IPMEmployeeAssignmentRepository _assignmentRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserTechnologyRepository _userTechnologyRepository;
        private readonly ILogger<EmployeeAssignmentService> _logger;

        public EmployeeAssignmentService(
            IPMEmployeeAssignmentRepository assignmentRepository,
            IProjectRepository projectRepository,
            IUserRepository userRepository,
            IUserTechnologyRepository userTechnologyRepository,
            ILogger<EmployeeAssignmentService> logger)
        {
            _assignmentRepository = assignmentRepository;
            _projectRepository = projectRepository;
            _userRepository = userRepository;
            _userTechnologyRepository = userTechnologyRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<EmployeeDto>> GetAvailableEmployeesAsync(EmployeeFilterDto? filter = null)
        {
            _logger.LogInformation("Getting available employees with filter");

            var users = await _userRepository.GetAllUsersAsync();
            
            // Filter for active employees only
            var employees = users.Where(u => u.Status == "active").ToList();
            
            var employeeDtos = new List<EmployeeDto>();

            foreach (var employee in employees)
            {
                var workload = await _assignmentRepository.GetEmployeeCurrentWorkloadAsync(employee.Id);
                var skills = await GetEmployeeSkillsAsync(employee.Id);
                var assignments = await _assignmentRepository.GetEmployeeAssignmentsWithProjectsAsync(employee.Id);
                
                var employeeDto = new EmployeeDto
                {
                    Id = employee.Id,
                    Name = employee.Name,
                    Email = employee.Email,
                    Role = employee.JobRole,
                    Department = employee.Department,
                    Status = employee.Status,
                    CurrentWorkloadPercentage = workload,
                    AvailableWorkloadPercentage = Math.Max(0, 100 - workload),
                    Skills = skills.ToList(),
                    ActiveProjects = assignments.Select(a => a.Project.Name).Distinct().ToList(),
                    CurrentAssignments = assignments.Select(MapToAssignmentDto).ToList()
                };

                employeeDtos.Add(employeeDto);
            }

            // Apply filters if provided
            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    var searchLower = filter.SearchTerm.ToLower();
                    employeeDtos = employeeDtos.Where(e => 
                        e.Name.ToLower().Contains(searchLower) ||
                        e.Email.ToLower().Contains(searchLower) ||
                        e.Role.ToLower().Contains(searchLower) ||
                        e.Id.ToLower().Contains(searchLower)
                    ).ToList();
                }

                if (filter.RequiredSkills?.Any() == true)
                {
                    employeeDtos = employeeDtos.Where(e =>
                        filter.RequiredSkills.All(skill =>
                            e.Skills.Any(empSkill => 
                                empSkill.Equals(skill, StringComparison.OrdinalIgnoreCase))
                        )
                    ).ToList();
                }

                if (!string.IsNullOrEmpty(filter.Department))
                {
                    employeeDtos = employeeDtos.Where(e => 
                        e.Department.Equals(filter.Department, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }

                if (filter.AvailableOnly == true)
                {
                    employeeDtos = employeeDtos.Where(e => e.AvailableWorkloadPercentage > 0).ToList();
                }

                if (filter.MinAvailableWorkload.HasValue)
                {
                    employeeDtos = employeeDtos.Where(e => 
                        e.AvailableWorkloadPercentage >= filter.MinAvailableWorkload.Value
                    ).ToList();
                }
            }

            return employeeDtos.OrderBy(e => e.Name);
        }

        public async Task<IEnumerable<EmployeeDto>> GetEmployeesWithMatchingSkillsAsync(List<string> requiredSkills)
        {
            _logger.LogInformation($"Getting employees with matching skills: {string.Join(", ", requiredSkills)}");

            var filter = new EmployeeFilterDto
            {
                RequiredSkills = requiredSkills
            };

            return await GetAvailableEmployeesAsync(filter);
        }

        public async Task<EmployeeDto> GetEmployeeByIdAsync(string employeeId)
        {
            var user = await _userRepository.GetUserByIdAsync(employeeId);
            if (user == null)
                throw new KeyNotFoundException($"Employee with ID {employeeId} not found");

            var workload = await _assignmentRepository.GetEmployeeCurrentWorkloadAsync(employeeId);
            var skills = await GetEmployeeSkillsAsync(employeeId);
            var assignments = await _assignmentRepository.GetEmployeeAssignmentsWithProjectsAsync(employeeId);

            return new EmployeeDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.JobRole,
                Department = user.Department,
                Status = user.Status,
                CurrentWorkloadPercentage = workload,
                AvailableWorkloadPercentage = Math.Max(0, 100 - workload),
                Skills = skills.ToList(),
                ActiveProjects = assignments.Select(a => a.Project.Name).Distinct().ToList(),
                CurrentAssignments = assignments.Select(MapToAssignmentDto).ToList()
            };
        }

        public async Task<EmployeeWorkloadDto> GetEmployeeWorkloadAsync(string employeeId)
        {
            var user = await _userRepository.GetUserByIdAsync(employeeId);
            if (user == null)
                throw new KeyNotFoundException($"Employee with ID {employeeId} not found");

            var assignments = await _assignmentRepository.GetEmployeeAssignmentsWithProjectsAsync(employeeId);
            var totalWorkload = assignments.Sum(a => a.WorkloadPercentage);

            var projectWorkloads = assignments
                .GroupBy(a => new { a.ProjectId, a.Project.Name })
                .Select(g => new ProjectWorkloadDto
                {
                    ProjectId = g.Key.ProjectId,
                    ProjectName = g.Key.Name,
                    WorkloadPercentage = g.Sum(a => a.WorkloadPercentage),
                    Role = string.Join(", ", g.Select(a => a.Role).Distinct())
                })
                .ToList();

            return new EmployeeWorkloadDto
            {
                EmployeeId = employeeId,
                EmployeeName = user.Name,
                TotalWorkloadPercentage = totalWorkload,
                AvailableWorkloadPercentage = Math.Max(0, 100 - totalWorkload),
                ProjectWorkloads = projectWorkloads
            };
        }

        public async Task<EmployeeAssignmentDto> AssignEmployeeToProjectAsync(CreateEmployeeAssignmentDto request)
        {
            _logger.LogInformation($"Assigning employee {request.EmployeeId} to project {request.ProjectId}");

            // Validate assignment
            if (!await ValidateAssignmentAsync(request.EmployeeId, request.WorkloadPercentage))
            {
                throw new InvalidOperationException("Assignment validation failed - employee would be over-allocated");
            }

            // Check for duplicate assignment
            if (await _assignmentRepository.HasDuplicateAssignmentAsync(request.ProjectId, request.EmployeeId, request.Role))
            {
                throw new InvalidOperationException("Employee is already assigned to this project with this role");
            }

            var assignment = new PMEmployeeAssignment
            {
                ProjectId = request.ProjectId,
                EmployeeId = request.EmployeeId,
                Role = request.Role,
                WorkloadPercentage = request.WorkloadPercentage,
                AssignedDate = DateTime.UtcNow
            };

            var createdAssignment = await _assignmentRepository.AddAsync(assignment);
            return MapToAssignmentDto(createdAssignment);
        }

        public async Task<IEnumerable<EmployeeAssignmentDto>> AssignMultipleEmployeesToProjectAsync(BulkAssignEmployeesDto request)
        {
            _logger.LogInformation($"Bulk assigning {request.Assignments.Count} employees to project {request.ProjectId}");

            var assignments = new List<PMEmployeeAssignment>();
            var validationErrors = new List<string>();

            foreach (var assignmentRequest in request.Assignments)
            {
                // Validate each assignment
                if (!await ValidateAssignmentAsync(assignmentRequest.EmployeeId, assignmentRequest.WorkloadPercentage))
                {
                    var user = await _userRepository.GetUserByIdAsync(assignmentRequest.EmployeeId);
                    validationErrors.Add($"Employee {user?.Name ?? assignmentRequest.EmployeeId} would be over-allocated");
                    continue;
                }

                // Check for duplicate assignment
                if (await _assignmentRepository.HasDuplicateAssignmentAsync(request.ProjectId, assignmentRequest.EmployeeId, assignmentRequest.Role))
                {
                    var user = await _userRepository.GetUserByIdAsync(assignmentRequest.EmployeeId);
                    validationErrors.Add($"Employee {user?.Name ?? assignmentRequest.EmployeeId} is already assigned to this project with role {assignmentRequest.Role}");
                    continue;
                }

                assignments.Add(new PMEmployeeAssignment
                {
                    ProjectId = request.ProjectId,
                    EmployeeId = assignmentRequest.EmployeeId,
                    Role = assignmentRequest.Role,
                    WorkloadPercentage = assignmentRequest.WorkloadPercentage,
                    AssignedDate = DateTime.UtcNow
                });
            }

            if (validationErrors.Any())
            {
                throw new InvalidOperationException($"Validation errors: {string.Join("; ", validationErrors)}");
            }

            if (!assignments.Any())
            {
                throw new InvalidOperationException("No valid assignments to process");
            }

            var createdAssignments = await _assignmentRepository.AddRangeAsync(assignments);
            return createdAssignments.Select(MapToAssignmentDto);
        }

        public async Task<EmployeeAssignmentDto?> UpdateEmployeeAssignmentAsync(int assignmentId, UpdateEmployeeAssignmentDto request)
        {
            _logger.LogInformation($"Updating assignment {assignmentId}");

            var existingAssignment = await _assignmentRepository.GetByIdAsync(assignmentId);
            if (existingAssignment == null)
            {
                return null;
            }

            // Calculate the workload change
            var currentWorkload = await _assignmentRepository.GetEmployeeCurrentWorkloadAsync(existingAssignment.EmployeeId);
            var workloadWithoutThisAssignment = currentWorkload - existingAssignment.WorkloadPercentage;
            var newTotalWorkload = workloadWithoutThisAssignment + request.WorkloadPercentage;

            // Validate that the new workload doesn't exceed 100%
            if (newTotalWorkload > 100)
            {
                throw new InvalidOperationException($"Updated workload would exceed 100%. Current: {workloadWithoutThisAssignment}%, Requested: {request.WorkloadPercentage}%, Total: {newTotalWorkload}%");
            }

            // Update the assignment
            existingAssignment.Role = request.Role;
            existingAssignment.WorkloadPercentage = request.WorkloadPercentage;

            var updatedAssignment = await _assignmentRepository.UpdateAsync(existingAssignment);
            return MapToAssignmentDto(updatedAssignment);
        }

        public async Task<bool> RemoveEmployeeFromProjectAsync(int assignmentId)
        {
            _logger.LogInformation($"Removing assignment {assignmentId}");
            return await _assignmentRepository.DeleteAsync(assignmentId);
        }

        public async Task<bool> RemoveEmployeeFromProjectByIdsAsync(string projectId, string employeeId)
        {
            _logger.LogInformation($"Removing employee {employeeId} from project {projectId}");
            return await _assignmentRepository.DeleteByProjectAndEmployeeAsync(projectId, employeeId);
        }

        public async Task<IEnumerable<EmployeeAssignmentDto>> GetProjectAssignmentsAsync(string projectId)
        {
            var assignments = await _assignmentRepository.GetByProjectIdAsync(projectId);
            return assignments.Select(MapToAssignmentDto);
        }

        public async Task<IEnumerable<EmployeeAssignmentDto>> GetEmployeeAssignmentsAsync(string employeeId)
        {
            var assignments = await _assignmentRepository.GetByEmployeeIdAsync(employeeId);
            return assignments.Select(MapToAssignmentDto);
        }

        public async Task<bool> ValidateAssignmentAsync(string employeeId, int workloadPercentage)
        {
            var currentWorkload = await _assignmentRepository.GetEmployeeCurrentWorkloadAsync(employeeId);
            var totalAfterAssignment = currentWorkload + workloadPercentage;
            
            return totalAfterAssignment <= 100;
        }

        public async Task<IEnumerable<string>> GetEmployeeSkillsAsync(string employeeId)
        {
            try
            {
                var userTechnologies = await _userTechnologyRepository.GetUserTechnologiesAsync(employeeId);
                return userTechnologies.Select(ut => ut.Technology.Name).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Error getting skills for employee {employeeId}");
                return new List<string>();
            }
        }

        private static EmployeeAssignmentDto MapToAssignmentDto(PMEmployeeAssignment assignment)
        {
            return new EmployeeAssignmentDto
            {
                Id = assignment.Id,
                ProjectId = assignment.ProjectId,
                ProjectName = assignment.Project?.Name ?? string.Empty,
                EmployeeId = assignment.EmployeeId,
                EmployeeName = assignment.Employee?.Name ?? string.Empty,
                Role = assignment.Role,
                WorkloadPercentage = assignment.WorkloadPercentage,
                AssignedDate = assignment.AssignedDate
            };
        }
    }
}