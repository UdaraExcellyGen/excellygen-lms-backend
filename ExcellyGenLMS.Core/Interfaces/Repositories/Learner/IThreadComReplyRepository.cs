using ExcellyGenLMS.Core.Entities.Learner;
using System.Linq.Expressions;
using System.Collections.Generic;
using System;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.Learner
{
    public interface IThreadComReplyRepository
    {
        Task<IEnumerable<ThreadComReply>> GetAllAsync(Expression<Func<ThreadComReply, bool>>? filter = null,
                                                      Func<IQueryable<ThreadComReply>, IOrderedQueryable<ThreadComReply>>? orderBy = null,
                                                      string includeProperties = "");
        Task<ThreadComReply?> GetByIdAsync(int id, string includeProperties = ""); // CHANGED: string id to int id
        Task AddAsync(ThreadComReply entity);
        Task UpdateAsync(ThreadComReply entity);
        Task DeleteAsync(ThreadComReply entity);
        Task<bool> ExistsAsync(int id); // CHANGED: string id to int id
    }
}
