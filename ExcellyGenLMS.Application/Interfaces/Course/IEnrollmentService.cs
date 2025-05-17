// ExcellyGenLMS.Application/Interfaces/Course/IEnrollmentService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using ExcellyGenLMS.Application.DTOs.Course;

namespace ExcellyGenLMS.Application.Interfaces.Course
{
    public interface IEnrollmentService
    {
        Task<List<EnrollmentDto>> GetAllEnrollmentsAsync();
        Task<EnrollmentDto> GetEnrollmentByIdAsync(int id);
        Task<List<EnrollmentDto>> GetEnrollmentsByUserIdAsync(string userId);
        Task<List<EnrollmentDto>> GetEnrollmentsByCourseIdAsync(int courseId);
        Task<EnrollmentDto> CreateEnrollmentAsync(CreateEnrollmentDto createEnrollmentDto);
        Task<EnrollmentDto> UpdateEnrollmentAsync(int id, UpdateEnrollmentDto updateEnrollmentDto);
        Task DeleteEnrollmentAsync(int id);
    }
}