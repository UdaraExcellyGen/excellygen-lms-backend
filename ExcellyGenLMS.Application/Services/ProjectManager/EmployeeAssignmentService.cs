// Path: ExcellyGenLMS.Application/Services/ProjectManager/EmployeeAssignmentService.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using ExcellyGenLMS.Application.DTOs.ProjectManager;
using ExcellyGenLMS.Application.Interfaces.ProjectManager;
using ExcellyGenLMS.Application.Interfaces.Learner;
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
        private readonly ILearnerNotificationService _notificationService;
        private readonly ILogger<EmployeeAssignmentService> _logger;

        public EmployeeAssignmentService(
            IPMEmployeeAssignmentRepository assignmentRepository,
            IProjectRepository projectRepository,
            IUserRepository userRepository,
            IUserTechnologyRepository userTechnologyRepository,
            ILearnerNotificationService notificationService,
            ILogger<EmployeeAssignmentService> logger)
        {
            _assignmentRepository = assignmentRepository;
            _projectRepository = projectRepository;
            _userRepository = userRepository;
            _userTechnologyRepository = userTechnologyRepository;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<IEnumerable<EmployeeDto>> GetAvailableEmployeesAsync(EmployeeFilterDto? filter = null)
        {
            _logger.LogInformation("Getting available employees with filter - OPTIMIZED VERSION");

            var users = await _userRepository.GetAllUsersAsync();

            // Filter for active employees only
            var employees = users.Where(u => u.Status == "active").ToList();
            var employeeIds = employees.Select(e => e.Id).ToList();

            _logger.LogInformation($"Loading bulk data for {employeeIds.Count} employees");

            // OPTIMIZATION: Load all related data in bulk queries instead of N+1 individual queries
            var bulkWorkloads = await _assignmentRepository.GetEmployeesCurrentWorkloadAsync(employeeIds);
            var bulkSkills = await _userTechnologyRepository.GetSkillsForMultipleUsersAsync(employeeIds);
            var bulkAssignments = await _assignmentRepository.GetEmployeesAssignmentsWithProjectsAsync(employeeIds);
            var bulkActiveProjects = await _assignmentRepository.GetEmployeesActiveProjectNamesAsync(employeeIds);

            _logger.LogInformation("Bulk data loaded successfully, building employee DTOs");

            var employeeDtos = new List<EmployeeDto>();

            foreach (var employee in employees)
            {
                var workload = bulkWorkloads.GetValueOrDefault(employee.Id, 0);
                var skills = bulkSkills.GetValueOrDefault(employee.Id, new List<string>());
                var assignments = bulkAssignments.GetValueOrDefault(employee.Id, new List<PMEmployeeAssignment>());
                var activeProjects = bulkActiveProjects.GetValueOrDefault(employee.Id, new List<string>());

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
                    Skills = skills,
                    ActiveProjects = activeProjects,
                    CurrentAssignments = assignments.Select(MapToAssignmentDto).ToList()
                };

                employeeDtos.Add(employeeDto);
            }

            // Apply filters if provided
            var filteredEmployees = ApplyFilters(employeeDtos, filter);

            _logger.LogInformation($"Returning {filteredEmployees.Count()} filtered employees");
            return filteredEmployees;
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

            // Load related data for single employee
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

            // Get project and user details for notification
            var project = await _projectRepository.GetByIdAsync(request.ProjectId);
            var employee = await _userRepository.GetUserByIdAsync(request.EmployeeId);

            if (project == null || employee == null)
            {
                throw new InvalidOperationException("Project or employee not found");
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

            // Create notification for the assigned employee
            try
            {
                await _notificationService.CreateProjectAssignmentNotificationAsync(
                    employeeId: request.EmployeeId,
                    projectId: request.ProjectId,
                    projectName: project.Name,
                    role: request.Role,
                    workloadPercentage: request.WorkloadPercentage,
                    assignerName: project.Creator?.Name ?? "Project Manager"
                );

                _logger.LogInformation($"Created assignment notification for employee {request.EmployeeId} on project {request.ProjectId}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to create notification for employee assignment {request.EmployeeId} to project {request.ProjectId}");
                // Don't fail the assignment if notification creation fails
            }

            return MapToAssignmentDto(createdAssignment);
        }

        public async Task<IEnumerable<EmployeeAssignmentDto>> AssignMultipleEmployeesToProjectAsync(BulkAssignEmployeesDto request)
        {
            _logger.LogInformation($"Bulk assigning {request.Assignments.Count} employees to project {request.ProjectId}");

            var assignments = new List<PMEmployeeAssignment>();
            var validationErrors = new List<string>();

            // Get project details for notifications
            var project = await _projectRepository.GetByIdAsync(request.ProjectId);
            if (project == null)
            {
                throw new InvalidOperationException("Project not found");
            }

            // Validate all assignments first
            var employeeIds = request.Assignments.Select(a => a.EmployeeId).ToList();
            var currentWorkloads = await _assignmentRepository.GetEmployeesCurrentWorkloadAsync(employeeIds);

            foreach (var assignmentRequest in request.Assignments)
            {
                var currentWorkload = currentWorkloads.GetValueOrDefault(assignmentRequest.EmployeeId, 0);
                var newTotalWorkload = currentWorkload + assignmentRequest.WorkloadPercentage;

                // Validate workload
                if (newTotalWorkload > 100)
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

            // Create notifications for all assigned employees
            foreach (var assignment in assignments)
            {
                try
                {
                    await _notificationService.CreateProjectAssignmentNotificationAsync(
                        employeeId: assignment.EmployeeId,
                        projectId: assignment.ProjectId,
                        projectName: project.Name,
                        role: assignment.Role,
                        workloadPercentage: assignment.WorkloadPercentage,
                        assignerName: project.Creator?.Name ?? "Project Manager"
                    );

                    _logger.LogInformation($"Created assignment notification for employee {assignment.EmployeeId} on project {assignment.ProjectId}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Failed to create notification for employee assignment {assignment.EmployeeId} to project {assignment.ProjectId}");
                    // Don't fail the assignment if notification creation fails
                }
            }

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

            // Get project details for notification
            var project = await _projectRepository.GetByIdAsync(existingAssignment.ProjectId);

            // Update the assignment
            existingAssignment.Role = request.Role;
            existingAssignment.WorkloadPercentage = request.WorkloadPercentage;

            var updatedAssignment = await _assignmentRepository.UpdateAsync(existingAssignment);

            // Create notification for the updated assignment
            if (project != null)
            {
                try
                {
                    await _notificationService.CreateProjectUpdateNotificationAsync(
                        employeeId: existingAssignment.EmployeeId,
                        projectId: existingAssignment.ProjectId,
                        projectName: project.Name,
                        updateType: "Role and workload updated",
                        assignerName: project.Creator?.Name ?? "Project Manager"
                    );

                    _logger.LogInformation($"Created update notification for assignment {assignmentId}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Failed to create notification for assignment update {assignmentId}");
                    // Don't fail the update if notification creation fails
                }
            }

            return MapToAssignmentDto(updatedAssignment);
        }

        public async Task<bool> RemoveEmployeeFromProjectAsync(int assignmentId)
        {
            _logger.LogInformation($"Removing assignment {assignmentId}");

            var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);
            if (assignment == null) return false;

            // Get project details for notification
            var project = await _projectRepository.GetByIdAsync(assignment.ProjectId);

            var result = await _assignmentRepository.DeleteAsync(assignmentId);

            if (result)
            {
                // Create notification for the removed assignment
                if (project != null)
                {
                    try
                    {
                        await _notificationService.CreateProjectRemovalNotificationAsync(
                            employeeId: assignment.EmployeeId,
                            projectId: assignment.ProjectId,
                            projectName: project.Name,
                            assignerName: project.Creator?.Name ?? "Project Manager"
                        );

                        _logger.LogInformation($"Created removal notification for assignment {assignmentId}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Failed to create notification for assignment removal {assignmentId}");
                        // Don't fail the removal if notification creation fails
                    }
                }
            }

            return result;
        }

        public async Task<bool> RemoveEmployeeFromProjectByIdsAsync(string projectId, string employeeId)
        {
            _logger.LogInformation($"Removing employee {employeeId} from project {projectId}");

            // Get project details for notification
            var project = await _projectRepository.GetByIdAsync(projectId);

            var result = await _assignmentRepository.DeleteByProjectAndEmployeeAsync(projectId, employeeId);

            if (result)
            {
                // Create notification for the removed assignment
                if (project != null)
                {
                    try
                    {
                        await _notificationService.CreateProjectRemovalNotificationAsync(
                            employeeId: employeeId,
                            projectId: projectId,
                            projectName: project.Name,
                            assignerName: project.Creator?.Name ?? "Project Manager"
                        );

                        _logger.LogInformation($"Created removal notification for employee {employeeId} from project {projectId}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Failed to create notification for employee {employeeId} removal from project {projectId}");
                        // Don't fail the removal if notification creation fails
                    }
                }
            }

            return result;
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
                var skills = userTechnologies.Select(ut => ut.Technology.Name).ToList();
                return skills;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Error getting skills for employee {employeeId}");
                return new List<string>();
            }
        }

        // PRIVATE HELPER METHODS

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

        private static IEnumerable<EmployeeDto> ApplyFilters(IEnumerable<EmployeeDto> employees, EmployeeFilterDto? filter)
        {
            if (filter == null) return employees;

            var filtered = employees.AsEnumerable();

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                var searchLower = filter.SearchTerm.ToLower();
                filtered = filtered.Where(e =>
                    e.Name.ToLower().Contains(searchLower) ||
                    e.Email.ToLower().Contains(searchLower) ||
                    e.Role.ToLower().Contains(searchLower) ||
                    e.Id.ToLower().Contains(searchLower)
                );
            }

            if (filter.RequiredSkills?.Any() == true)
            {
                filtered = filtered.Where(e =>
                    filter.RequiredSkills.All(skill =>
                        e.Skills.Any(empSkill =>
                            empSkill.Equals(skill, StringComparison.OrdinalIgnoreCase))
                    )
                );
            }

            if (!string.IsNullOrEmpty(filter.Department))
            {
                filtered = filtered.Where(e =>
                    e.Department.Equals(filter.Department, StringComparison.OrdinalIgnoreCase)
                );
            }

            if (filter.AvailableOnly == true)
            {
                filtered = filtered.Where(e => e.AvailableWorkloadPercentage > 0);
            }

            if (filter.MinAvailableWorkload.HasValue)
            {
                filtered = filtered.Where(e =>
                    e.AvailableWorkloadPercentage >= filter.MinAvailableWorkload.Value
                );
            }

            return filtered.OrderBy(e => e.Name);
        }
    }
}