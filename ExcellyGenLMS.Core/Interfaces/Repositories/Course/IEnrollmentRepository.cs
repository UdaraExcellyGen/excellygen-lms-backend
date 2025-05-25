// ExcellyGenLMS.Core/Interfaces/Repositories/Course/IEnrollmentRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using ExcellyGenLMS.Core.Entities.Course;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.Course
{
    public interface IEnrollmentRepository
    {
        Task<List<Enrollment>> GetAllEnrollmentsAsync();
        Task<Enrollment?> GetEnrollmentByIdAsync(int id);
        Task<Enrollment?> GetEnrollmentByUserIdAndCourseIdAsync(string userId, int courseId);
        Task<List<Enrollment>> GetEnrollmentsByUserIdAsync(string userId);
        Task<List<Enrollment>> GetEnrollmentsByCourseIdAsync(int courseId);
        Task<Enrollment> CreateEnrollmentAsync(Enrollment enrollment);
        Task<Enrollment> UpdateEnrollmentAsync(Enrollment enrollment);
        Task DeleteEnrollmentAsync(int id);
        Task<int> GetTotalUniqueActiveLearnersCountAsync();
    }
}