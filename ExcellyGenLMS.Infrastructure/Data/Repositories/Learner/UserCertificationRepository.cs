using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExcellyGenLMS.Core.Entities.Learner;
using ExcellyGenLMS.Core.Interfaces.Repositories.Learner;
using ExcellyGenLMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ExcellyGenLMS.Infrastructure.Data.Repositories.Learner
{
    public class UserCertificationRepository : IUserCertificationRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserCertificationRepository> _logger;

        public UserCertificationRepository(
            ApplicationDbContext context,
            ILogger<UserCertificationRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<UserCertification>> GetUserCertificationsAsync(string userId)
        {
            return await _context.UserCertifications
                .Include(uc => uc.Certification)
                .Where(uc => uc.UserId == userId)
                .OrderByDescending(uc => uc.IssueDate)
                .ToListAsync();
        }

        public async Task<UserCertification> GetUserCertificationByIdAsync(string userId, string certificationId)
        {
            var userCertification = await _context.UserCertifications
                .Include(uc => uc.Certification)
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CertificationId == certificationId);

            return userCertification
                ?? throw new KeyNotFoundException($"Certification with ID {certificationId} not found for user with ID {userId}");
        }
    }
}