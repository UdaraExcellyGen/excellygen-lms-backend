// Path: ExcellyGenLMS.API/Controllers/ProjectManager/EmployeeAssignmentController.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ExcellyGenLMS.Application.DTOs.ProjectManager;
using ExcellyGenLMS.Application.Interfaces.ProjectManager;

namespace ExcellyGenLMS.API.Controllers.ProjectManager
{
    [ApiController]
    [Authorize(Roles = "Admin,ProjectManager")]
    [Route("api/project-manager")]
    public class EmployeeAssignmentController : ControllerBase
    {
        private readonly IEmployeeAssignmentService _employeeAssignmentService;
        private readonly ILogger<EmployeeAssignmentController> _logger;

        public EmployeeAssignmentController(
            IEmployeeAssignmentService employeeAssignmentService,
            ILogger<EmployeeAssignmentController> logger)
        {
            _employeeAssignmentService = employeeAssignmentService;
            _logger = logger;
        }

        // ----- Employee Endpoints -----

        [HttpGet("employees")]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAvailableEmployees(
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? department = null,
            [FromQuery] bool? availableOnly = null,
            [FromQuery] int? minAvailableWorkload = null,
            [FromQuery] List<string>? skills = null)
        {
            try
            {
                var filter = new EmployeeFilterDto
                {
                    SearchTerm = searchTerm,
                    Department = department,
                    AvailableOnly = availableOnly,
                    MinAvailableWorkload = minAvailableWorkload,
                    RequiredSkills = skills
                };

                var employees = await _employeeAssignmentService.GetAvailableEmployeesAsync(filter);
                return Ok(employees);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching available employees");
                return StatusCode(500, new { message = "An error occurred while fetching employees", error = ex.Message });
            }
        }

        [HttpGet("employees/{id}")]
        public async Task<ActionResult<EmployeeDto>> GetEmployee(string id)
        {
            try
            {
                var employee = await _employeeAssignmentService.GetEmployeeByIdAsync(id);
                return Ok(employee);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching employee {id}");
                return StatusCode(500, new { message = $"An error occurred while fetching employee {id}", error = ex.Message });
            }
        }

        [HttpGet("employees/{id}/workload")]
        public async Task<ActionResult<EmployeeWorkloadDto>> GetEmployeeWorkload(string id)
        {
            try
            {
                var workload = await _employeeAssignmentService.GetEmployeeWorkloadAsync(id);
                return Ok(workload);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching workload for employee {id}");
                return StatusCode(500, new { message = $"An error occurred while fetching employee workload", error = ex.Message });
            }
        }

        [HttpGet("employees/by-skills")]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployeesBySkills([FromQuery] List<string> skills)
        {
            try
            {
                if (!skills.Any())
                {
                    return BadRequest(new { message = "At least one skill must be specified" });
                }

                var employees = await _employeeAssignmentService.GetEmployeesWithMatchingSkillsAsync(skills);
                return Ok(employees);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching employees by skills");
                return StatusCode(500, new { message = "An error occurred while fetching employees by skills", error = ex.Message });
            }
        }

        // ----- Assignment Endpoints -----

        [HttpPost("employee-assignments")]
        public async Task<ActionResult<EmployeeAssignmentDto>> AssignEmployeeToProject([FromBody] CreateEmployeeAssignmentDto request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ProjectId) || string.IsNullOrEmpty(request.EmployeeId) || string.IsNullOrEmpty(request.Role))
                {
                    return BadRequest(new { message = "ProjectId, EmployeeId, and Role are required" });
                }

                if (request.WorkloadPercentage <= 0 || request.WorkloadPercentage > 100)
                {
                    return BadRequest(new { message = "WorkloadPercentage must be between 1 and 100" });
                }

                var assignment = await _employeeAssignmentService.AssignEmployeeToProjectAsync(request);
                return CreatedAtAction(nameof(GetProjectAssignments), new { projectId = request.ProjectId }, assignment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning employee to project");
                return StatusCode(500, new { message = "An error occurred while assigning employee to project", error = ex.Message });
            }
        }

        [HttpPost("employee-assignments/bulk")]
        public async Task<ActionResult<IEnumerable<EmployeeAssignmentDto>>> AssignMultipleEmployeesToProject([FromBody] BulkAssignEmployeesDto request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ProjectId))
                {
                    return BadRequest(new { message = "ProjectId is required" });
                }

                if (!request.Assignments.Any())
                {
                    return BadRequest(new { message = "At least one assignment is required" });
                }

                var assignments = await _employeeAssignmentService.AssignMultipleEmployeesToProjectAsync(request);
                return Ok(assignments);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk assigning employees to project");
                return StatusCode(500, new { message = "An error occurred while bulk assigning employees", error = ex.Message });
            }
        }

        [HttpPut("employee-assignments/{id}")]
        public async Task<ActionResult<EmployeeAssignmentDto>> UpdateEmployeeAssignment(int id, [FromBody] UpdateEmployeeAssignmentDto request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Role))
                {
                    return BadRequest(new { message = "Role is required" });
                }

                if (request.WorkloadPercentage <= 0 || request.WorkloadPercentage > 100)
                {
                    return BadRequest(new { message = "WorkloadPercentage must be between 1 and 100" });
                }

                var assignment = await _employeeAssignmentService.UpdateEmployeeAssignmentAsync(id, request);
                if (assignment == null)
                {
                    return NotFound(new { message = $"Assignment with ID {id} not found" });
                }

                return Ok(assignment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating assignment {id}");
                return StatusCode(500, new { message = "An error occurred while updating assignment", error = ex.Message });
            }
        }

        [HttpDelete("employee-assignments/{id}")]
        public async Task<ActionResult> RemoveEmployeeAssignment(int id)
        {
            try
            {
                var result = await _employeeAssignmentService.RemoveEmployeeFromProjectAsync(id);
                if (!result)
                {
                    return NotFound(new { message = $"Assignment with ID {id} not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing assignment {id}");
                return StatusCode(500, new { message = $"An error occurred while removing assignment", error = ex.Message });
            }
        }

        [HttpDelete("projects/{projectId}/employees/{employeeId}")]
        public async Task<ActionResult> RemoveEmployeeFromProject(string projectId, string employeeId)
        {
            try
            {
                var result = await _employeeAssignmentService.RemoveEmployeeFromProjectByIdsAsync(projectId, employeeId);
                if (!result)
                {
                    return NotFound(new { message = $"No assignment found for employee {employeeId} in project {projectId}" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing employee {employeeId} from project {projectId}");
                return StatusCode(500, new { message = "An error occurred while removing employee from project", error = ex.Message });
            }
        }

        [HttpGet("projects/{projectId}/assignments")]
        public async Task<ActionResult<IEnumerable<EmployeeAssignmentDto>>> GetProjectAssignments(string projectId)
        {
            try
            {
                var assignments = await _employeeAssignmentService.GetProjectAssignmentsAsync(projectId);
                return Ok(assignments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching assignments for project {projectId}");
                return StatusCode(500, new { message = "An error occurred while fetching project assignments", error = ex.Message });
            }
        }

        [HttpGet("employees/{employeeId}/assignments")]
        public async Task<ActionResult<IEnumerable<EmployeeAssignmentDto>>> GetEmployeeAssignments(string employeeId)
        {
            try
            {
                var assignments = await _employeeAssignmentService.GetEmployeeAssignmentsAsync(employeeId);
                return Ok(assignments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching assignments for employee {employeeId}");
                return StatusCode(500, new { message = "An error occurred while fetching employee assignments", error = ex.Message });
            }
        }

        // ----- Validation Endpoints -----

        [HttpPost("validate-assignment")]
        public async Task<ActionResult<bool>> ValidateAssignment([FromBody] CreateEmployeeAssignmentDto request)
        {
            try
            {
                var isValid = await _employeeAssignmentService.ValidateAssignmentAsync(request.EmployeeId, request.WorkloadPercentage);
                return Ok(new { isValid = isValid });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating assignment");
                return StatusCode(500, new { message = "An error occurred while validating assignment", error = ex.Message });
            }
        }
    }
}