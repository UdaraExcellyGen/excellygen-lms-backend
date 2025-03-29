using System;
using System.ComponentModel.DataAnnotations;

namespace ExcellyGenLMS.Core.Entities.Admin
{
	public class CourseCategory
	{
		[Key]
		public string Id { get; set; } = Guid.NewGuid().ToString();

		[Required]
		[StringLength(100)]
		public string Title { get; set; } = string.Empty;

		[StringLength(500)]
		public string Description { get; set; } = string.Empty;

		[StringLength(100)]
		public string Icon { get; set; } = string.Empty;

		[Required]
		[StringLength(20)]
		public string Status { get; set; } = "active"; // active or inactive
	}
}