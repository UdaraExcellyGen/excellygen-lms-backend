using ExcellyGenLMS.Application.DTOs.Course;
using ExcellyGenLMS.Application.DTOs.Admin;
namespace ExcellyGenLMS.Application.Interfaces.Admin
{
	public interface ICourseAdminService
	{
		Task<List<CourseDto>> GetCoursesByCategoryIdAsync(string categoryId);
		Task<CourseDto> UpdateCourseAdminAsync(int id, UpdateCourseAdminDto updateCourseDto);
		Task DeleteCourseAsync(int id);
	}
}