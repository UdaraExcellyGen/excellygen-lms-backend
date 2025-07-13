// ExcellyGenLMS.Infrastructure/Data/Repositories/Course/ExternalCertificateRepository.cs
using ExcellyGenLMS.Core.Entities.Course;
using ExcellyGenLMS.Core.Interfaces.Repositories.Course;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Infrastructure.Data.Repositories.Course
{
    public class ExternalCertificateRepository : IExternalCertificateRepository
    {
        private readonly ApplicationDbContext _context;

        public ExternalCertificateRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ExternalCertificate?> GetByIdAsync(string id)
        {
            return await _context.ExternalCertificates
                                 .Include(ec => ec.User)
                                 .FirstOrDefaultAsync(ec => ec.Id == id);
        }

        public async Task<IEnumerable<ExternalCertificate>> GetByUserIdAsync(string userId)
        {
            return await _context.ExternalCertificates
                                 .Where(ec => ec.UserId == userId)
                                 .Include(ec => ec.User)
                                 .OrderByDescending(ec => ec.CompletionDate)
                                 .ToListAsync();
        }

        public async Task<ExternalCertificate> AddAsync(ExternalCertificate externalCertificate)
        {
            externalCertificate.CreatedAt = DateTime.UtcNow;
            externalCertificate.UpdatedAt = DateTime.UtcNow;

            _context.ExternalCertificates.Add(externalCertificate);
            await _context.SaveChangesAsync();

            // Reload to include navigation properties
            return await GetByIdAsync(externalCertificate.Id) ?? externalCertificate;
        }

        public async Task<ExternalCertificate> UpdateAsync(ExternalCertificate externalCertificate)
        {
            externalCertificate.UpdatedAt = DateTime.UtcNow;

            _context.Entry(externalCertificate).State = EntityState.Modified;
            _context.Entry(externalCertificate).Property(e => e.CreatedAt).IsModified = false; // Don't update CreatedAt

            await _context.SaveChangesAsync();

            // Reload to include navigation properties
            return await GetByIdAsync(externalCertificate.Id) ?? externalCertificate;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var externalCertificate = await _context.ExternalCertificates.FindAsync(id);
            if (externalCertificate == null)
            {
                return false;
            }

            _context.ExternalCertificates.Remove(externalCertificate);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.ExternalCertificates.AnyAsync(ec => ec.Id == id);
        }

        public async Task<bool> UserOwnsExternalCertificateAsync(string userId, string certificateId)
        {
            return await _context.ExternalCertificates
                                 .AnyAsync(ec => ec.Id == certificateId && ec.UserId == userId);
        }
    }
}