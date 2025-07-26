using ExcellyGenLMS.Core.Entities.Course;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.Course
{
    public interface IDocumentProgressRepository
    {
        Task<DocumentProgress?> GetProgressByUserIdAndDocumentIdAsync(string userId, int documentId);
        Task<IEnumerable<DocumentProgress>> GetProgressByUserIdAndCourseIdsAsync(string userId, List<int> courseIds);
        Task<DocumentProgress> AddAsync(DocumentProgress progress);
        Task UpdateAsync(DocumentProgress progress);
    }
}