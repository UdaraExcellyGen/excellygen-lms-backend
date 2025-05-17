using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.Admin
{
    public interface ICourseAdminRepository
    {
        Task<List<ExcellyGenLMS.Core.Entities.Course.Course>> GetCoursesByCategoryIdAsync(string categoryId);
        Task<ExcellyGenLMS.Core.Entities.Course.Course?> GetCourseByIdAsync(int id);
        Task<ExcellyGenLMS.Core.Entities.Course.Course> UpdateCourseAsync(ExcellyGenLMS.Core.Entities.Course.Course course);
        Task DeleteCourseAsync(int id);
    }
}