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
    public class ThreadComReplyRepository : IThreadComReplyRepository
    {
        private readonly ApplicationDbContext _context;

        public ThreadComReplyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ThreadComReply>> GetAllAsync(
            Expression<Func<ThreadComReply, bool>>? filter = null,
            Func<IQueryable<ThreadComReply>, IOrderedQueryable<ThreadComReply>>? orderBy = null,
            string includeProperties = "")
        {
            IQueryable<ThreadComReply> query = _context.ThreadComReplies;

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

        public async Task<ThreadComReply?> GetByIdAsync(int id, string includeProperties = "") // CHANGED parameter type
        {
            IQueryable<ThreadComReply> query = _context.ThreadComReplies;
            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty.Trim());
            }
            return await query.FirstOrDefaultAsync(e => e.Id == id); // Comparing int to int
        }

        public async Task AddAsync(ThreadComReply entity)
        {
            await _context.ThreadComReplies.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ThreadComReply entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ThreadComReply entity)
        {
            _context.ThreadComReplies.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id) // CHANGED parameter type
        {
            return await _context.ThreadComReplies.AnyAsync(e => e.Id == id); // Comparing int to int
        }
    }
}