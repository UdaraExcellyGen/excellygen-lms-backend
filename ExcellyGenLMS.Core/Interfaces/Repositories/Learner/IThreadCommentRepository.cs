using ExcellyGenLMS.Core.Entities.Learner;
using System.Linq.Expressions;
using System.Collections.Generic;
using System;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.Learner
{
    public interface IThreadCommentRepository
    {
        Task<IEnumerable<ThreadComment>> GetAllAsync(Expression<Func<ThreadComment, bool>>? filter = null,
                                                     Func<IQueryable<ThreadComment>, IOrderedQueryable<ThreadComment>>? orderBy = null,
                                                     string includeProperties = "");
        Task<ThreadComment?> GetByIdAsync(int id, string includeProperties = ""); // CHANGED: string id to int id
        Task AddAsync(ThreadComment entity);
        Task UpdateAsync(ThreadComment entity);
        Task DeleteAsync(ThreadComment entity);
        Task<bool> ExistsAsync(int id); // CHANGED: string id to int id
    }
}