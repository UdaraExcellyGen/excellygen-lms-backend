// Path: ExcellyGenLMS.Application/DTOs/ProjectManager/EmployeeAssignmentDTOs.cs

using System;
using System.Collections.Generic;

namespace ExcellyGenLMS.Application.DTOs.ProjectManager
{
    public class EmployeeDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int CompletedProjectsCount { get; set; } = 0;
        public int CurrentWorkloadPercentage { get; set; } = 0;
        public int AvailableWorkloadPercentage { get; set; } = 100;
        public List<string> Skills { get; set; } = new List<string>();
        public List<string> ActiveProjects { get; set; } = new List<string>();
        public List<EmployeeAssignmentDto> CurrentAssignments { get; set; } = new List<EmployeeAssignmentDto>();
    }

    public class EmployeeAssignmentDto
    {
        public int Id { get; set; }
        public string ProjectId { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int WorkloadPercentage { get; set; } = 100;
        public DateTime AssignedDate { get; set; }
    }

    public class CreateEmployeeAssignmentDto
    {
        public string ProjectId { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int WorkloadPercentage { get; set; } = 100;
    }

    public class BulkAssignEmployeesDto
    {
        public string ProjectId { get; set; } = string.Empty;
        public List<EmployeeAssignmentRequestDto> Assignments { get; set; } = new List<EmployeeAssignmentRequestDto>();
    }

    public class EmployeeAssignmentRequestDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int WorkloadPercentage { get; set; } = 100;
    }

    public class EmployeeWorkloadDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public int TotalWorkloadPercentage { get; set; } = 0;
        public int AvailableWorkloadPercentage { get; set; } = 100;
        public List<ProjectWorkloadDto> ProjectWorkloads { get; set; } = new List<ProjectWorkloadDto>();
    }

    public class ProjectWorkloadDto
    {
        public string ProjectId { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public int WorkloadPercentage { get; set; } = 0;
        public string Role { get; set; } = string.Empty;
    }

    public class EmployeeFilterDto
    {
        public List<string>? RequiredSkills { get; set; }
        public bool? AvailableOnly { get; set; }
        public string? SearchTerm { get; set; }
        public string? Department { get; set; }
        public int? MinAvailableWorkload { get; set; }
    }
}