using System.Collections.Generic;

namespace ExcellyGenLMS.Application.DTOs.Learner // <<<< CHANGED NAMESPACE
{
	public class CvPersonalInfo
	{
		public string Name { get; set; } = string.Empty;
		public string Position { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string Phone { get; set; } = string.Empty;
		public string Department { get; set; } = string.Empty;
		public string? Photo { get; set; }
		public string Summary { get; set; } = string.Empty;
	}

	public class CvProjectDto
	{
		public string Title { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public List<string> Technologies { get; set; } = new List<string>();
		public string StartDate { get; set; } = string.Empty;
		public string? CompletionDate { get; set; }
		public string Status { get; set; } = string.Empty;
	}

	public class CvCourseDto
	{
		public string Title { get; set; } = string.Empty;
		public string Provider { get; set; } = string.Empty;
		public string CompletionDate { get; set; } = string.Empty;
		public string? Duration { get; set; }
		public bool Certificate { get; set; } = true;
	}

	public class CvDto
	{
		public CvPersonalInfo PersonalInfo { get; set; } = new CvPersonalInfo();
		public List<CvProjectDto> Projects { get; set; } = new List<CvProjectDto>();
		public List<CvCourseDto> Courses { get; set; } = new List<CvCourseDto>();
		public List<string> Skills { get; set; } = new List<string>();
	}
}