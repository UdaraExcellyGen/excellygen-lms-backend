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
    public class ForumThreadRepository : IForumThreadRepository
    {
        private readonly ApplicationDbContext _context;

        public ForumThreadRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ForumThread>> GetThreadsAsync(
            Expression<Func<ForumThread, bool>>? filter = null,
            Func<IQueryable<ForumThread>, IOrderedQueryable<ForumThread>>? orderBy = null,
            int? skip = null,
            int? take = null,
            string includeProperties = "")
        {
            IQueryable<ForumThread> query = _context.ForumThreads;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty.Trim());
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }
            else
            {
                query = query.OrderByDescending(t => t.CreatedAt);
            }

            if (skip.HasValue)
            {
                query = query.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<int> CountThreadsAsync(Expression<Func<ForumThread, bool>>? filter = null)
        {
            IQueryable<ForumThread> query = _context.ForumThreads;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return await query.CountAsync();
        }

        public async Task<ForumThread?> GetByIdAsync(int id, string includeProperties = "") // CHANGED parameter type
        {
            IQueryable<ForumThread> query = _context.ForumThreads;
            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty.Trim());
            }
            return await query.FirstOrDefaultAsync(e => e.Id == id); // Comparing int to int
        }

        public async Task AddAsync(ForumThread entity)
        {
            await _context.ForumThreads.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ForumThread entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ForumThread entity)
        {
            _context.ForumThreads.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id) // CHANGED parameter type
        {
            return await _context.ForumThreads.AnyAsync(e => e.Id == id); // Comparing int to int
        }
    }
}