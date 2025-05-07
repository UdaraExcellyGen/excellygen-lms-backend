using ExcellyGenLMS.Core.Entities.Learner;
using ExcellyGenLMS.Core.Interfaces.Repositories.Learner;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Infrastructure.Data.Repositories.Learner
{
    public class ThreadCommentRepository : IThreadCommentRepository
    {
        private readonly ApplicationDbContext _context;

        public ThreadCommentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ThreadComment>> GetAllAsync(
            Expression<Func<ThreadComment, bool>>? filter = null,
            Func<IQueryable<ThreadComment>, IOrderedQueryable<ThreadComment>>? orderBy = null,
            string includeProperties = "")
        {
            IQueryable<ThreadComment> query = _context.ThreadComments;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty.Trim());
            }

            if (orderBy != null)
            {
                return await orderBy(query).ToListAsync();
            }
            else
            {
                return await query.ToListAsync();
            }
        }

        public async Task<ThreadComment?> GetByIdAsync(int id, string includeProperties = "") // CHANGED parameter type
        {
            IQueryable<ThreadComment> query = _context.ThreadComments;
            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty.Trim());
            }
            return await query.FirstOrDefaultAsync(e => e.Id == id); // Comparing int to int
        }

        public async Task AddAsync(ThreadComment entity)
        {
            await _context.ThreadComments.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ThreadComment entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ThreadComment entity)
        {
            _context.ThreadComments.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id) // CHANGED parameter type
        {
            return await _context.ThreadComments.AnyAsync(e => e.Id == id); // Comparing int to int
        }
    }
}