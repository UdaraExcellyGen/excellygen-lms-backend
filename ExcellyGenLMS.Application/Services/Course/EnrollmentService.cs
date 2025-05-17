// ExcellyGenLMS.Application/Services/Course/EnrollmentService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExcellyGenLMS.Application.DTOs.Course;
using ExcellyGenLMS.Application.Interfaces.Course;
using ExcellyGenLMS.Core.Entities.Course;
using ExcellyGenLMS.Core.Interfaces.Repositories.Course;

namespace ExcellyGenLMS.Application.Services.Course
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IEnrollmentRepository _enrollmentRepository;

        public EnrollmentService(IEnrollmentRepository enrollmentRepository)
        {
            _enrollmentRepository = enrollmentRepository;
        }

        public async Task<List<EnrollmentDto>> GetAllEnrollmentsAsync()
        {
            var enrollments = await _enrollmentRepository.GetAllEnrollmentsAsync();
            return enrollments.Select(MapToDto).ToList();
        }

        public async Task<EnrollmentDto> GetEnrollmentByIdAsync(int id)
        {
            var enrollment = await _enrollmentRepository.GetEnrollmentByIdAsync(id);
            return MapToDto(enrollment);
        }

        public async Task<List<EnrollmentDto>> GetEnrollmentsByUserIdAsync(string userId)
        {
            var enrollments = await _enrollmentRepository.GetEnrollmentsByUserIdAsync(userId);
            return enrollments.Select(MapToDto).ToList();
        }

        public async Task<List<EnrollmentDto>> GetEnrollmentsByCourseIdAsync(int courseId)
        {
            var enrollments = await _enrollmentRepository.GetEnrollmentsByCourseIdAsync(courseId);
            return enrollments.Select(MapToDto).ToList();
        }

        public async Task<EnrollmentDto> CreateEnrollmentAsync(CreateEnrollmentDto createEnrollmentDto)
        {
            var enrollment = new Enrollment
            {
                UserId = createEnrollmentDto.UserId,
                CourseId = createEnrollmentDto.CourseId,
                EnrollmentDate = DateTime.UtcNow,
                Status = createEnrollmentDto.Status
            };

            var createdEnrollment = await _enrollmentRepository.CreateEnrollmentAsync(enrollment);
            return MapToDto(createdEnrollment);
        }

        public async Task<EnrollmentDto> UpdateEnrollmentAsync(int id, UpdateEnrollmentDto updateEnrollmentDto)
        {
            var enrollment = await _enrollmentRepository.GetEnrollmentByIdAsync(id);
            enrollment.Status = updateEnrollmentDto.Status;

            var updatedEnrollment = await _enrollmentRepository.UpdateEnrollmentAsync(enrollment);
            return MapToDto(updatedEnrollment);
        }

        public async Task DeleteEnrollmentAsync(int id)
        {
            await _enrollmentRepository.DeleteEnrollmentAsync(id);
        }

        private static EnrollmentDto MapToDto(Enrollment enrollment)
        {
            return new EnrollmentDto
            {
                Id = enrollment.Id,
                UserId = enrollment.UserId,
                CourseId = enrollment.CourseId,
                EnrollmentDate = enrollment.EnrollmentDate,
                Status = enrollment.Status,
                UserName = enrollment.User?.Name ?? "Unknown",
                CourseTitle = enrollment.Course?.Title ?? "Unknown"
            };
        }
    }
}