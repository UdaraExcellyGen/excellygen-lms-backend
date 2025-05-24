using ExcellyGenLMS.Core.Entities.Learner;
using System.Linq.Expressions;
using System.Collections.Generic;
using System;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.Learner
{
    public interface IForumThreadRepository
    {
        Task<IEnumerable<ForumThread>> GetThreadsAsync(
            Expression<Func<ForumThread, bool>>? filter = null,
            Func<IQueryable<ForumThread>, IOrderedQueryable<ForumThread>>? orderBy = null,
            int? skip = null,
            int? take = null,
            string includeProperties = "");

        Task<int> CountThreadsAsync(Expression<Func<ForumThread, bool>>? filter = null);

        Task<ForumThread?> GetByIdAsync(int id, string includeProperties = ""); // CHANGED: string id to int id
        Task AddAsync(ForumThread entity);
        Task UpdateAsync(ForumThread entity);
        Task DeleteAsync(ForumThread entity);
        Task<bool> ExistsAsync(int id); // CHANGED: string id to int id
    }
}