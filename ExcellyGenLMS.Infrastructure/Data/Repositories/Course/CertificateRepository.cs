// ExcellyGenLMS.Infrastructure/Data/Repositories/Course/CertificateRepository.cs
using ExcellyGenLMS.Core.Entities.Course;
using ExcellyGenLMS.Core.Interfaces.Repositories.Course;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Infrastructure.Data.Repositories.Course
{
    public class CertificateRepository : ICertificateRepository
    {
        private readonly ApplicationDbContext _context;

        public CertificateRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Certificate?> GetByIdAsync(int id)
        {
            return await _context.Certificates
                                 .Include(c => c.User)
                                 .Include(c => c.Course)
                                 .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Certificate>> GetCertificatesByUserIdAsync(string userId)
        {
            return await _context.Certificates
                                 .Where(c => c.UserId == userId)
                                 .Include(c => c.User)
                                 .Include(c => c.Course)
                                 .ToListAsync();
        }

        public async Task<Certificate?> GetCertificateByUserIdAndCourseIdAsync(string userId, int courseId)
        {
            return await _context.Certificates
                                 .FirstOrDefaultAsync(c => c.UserId == userId && c.CourseId == courseId);
        }

        public async Task<Certificate> AddAsync(Certificate certificate)
        {
            _context.Certificates.Add(certificate);
            await _context.SaveChangesAsync();
            return certificate;
        }

        public async Task UpdateAsync(Certificate certificate)
        {
            _context.Entry(certificate).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var certificate = await _context.Certificates.FindAsync(id);
            if (certificate != null)
            {
                _context.Certificates.Remove(certificate);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException($"Certificate with ID {id} not found.");
            }
        }
    }
}