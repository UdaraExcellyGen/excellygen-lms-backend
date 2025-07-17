using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExcellyGenLMS.Core.Entities.Course;
using ExcellyGenLMS.Core.Interfaces.Repositories.Course;
using Microsoft.EntityFrameworkCore;
using ExcellyGenLMS.Infrastructure.Data;

namespace ExcellyGenLMS.Infrastructure.Data.Repositories.Course
{
    public class EnrollmentRepository : IEnrollmentRepository
    {
        private readonly ApplicationDbContext _context;

        public EnrollmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Enrollment>> GetAllEnrollmentsAsync()
        {
            return await _context.Enrollments
                                 .Include(e => e.User)
                                 .Include(e => e.Course)
                                     .ThenInclude(c => c!.Category)
                                 .ToListAsync();
        }

        public async Task<Enrollment?> GetEnrollmentByIdAsync(int id)
        {
            return await _context.Enrollments
                                 .Include(e => e.User)
                                 .Include(e => e.Course)
                                     .ThenInclude(c => c!.Category)
                                 .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Enrollment?> GetEnrollmentByUserIdAndCourseIdAsync(string userId, int courseId)
        {
            return await _context.Enrollments
                                 .Include(e => e.User)
                                 .Include(e => e.Course)
                                     .ThenInclude(c => c!.Category)
                                 .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);
        }

        public async Task<List<Enrollment>> GetEnrollmentsByUserIdAsync(string userId)
        {
            return await _context.Enrollments
                                 .Where(e => e.UserId == userId)
                                 .Include(e => e.User)
                                 .Include(e => e.Course)
                                     .ThenInclude(c => c!.Category)
                                 .ToListAsync();
        }

        public async Task<List<Enrollment>> GetEnrollmentsByCourseIdAsync(int courseId)
        {
            return await _context.Enrollments
                                 .Where(e => e.CourseId == courseId)
                                 .Include(e => e.User)
                                 .AsNoTracking()
                                 .ToListAsync();
        }

        public async Task<List<Enrollment>> GetEnrollmentsByCourseIdsAsync(List<int> courseIds)
        {
            if (courseIds == null || !courseIds.Any())
            {
                return new List<Enrollment>();
            }

            return await _context.Enrollments
                                 .Where(e => courseIds.Contains(e.CourseId))
                                 .AsNoTracking()
                                 .ToListAsync();
        }

        public async Task<Enrollment> CreateEnrollmentAsync(Enrollment enrollment)
        {
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();
            return enrollment;
        }

        public async Task<Enrollment> UpdateEnrollmentAsync(Enrollment enrollment)
        {
            _context.Entry(enrollment).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return enrollment;
        }

        public async Task DeleteEnrollmentAsync(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment != null)
            {
                _context.Enrollments.Remove(enrollment);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException($"Enrollment with ID {id} not found.");
            }
        }

        public async Task<int> GetTotalUniqueActiveLearnersCountAsync()
        {
            return await _context.Enrollments
                                 .Select(e => e.UserId)
                                 .Distinct()
                                 .CountAsync();
        }
    }
}