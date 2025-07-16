using ExcellyGenLMS.Application.DTOs;
using ExcellyGenLMS.Application.DTOs.Learner; // ADDED
using System.Collections.Generic;             // ADDED
using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Interfaces.Learner
{
    public interface ILearnerStatsService
    {
        Task<OverallLmsStatsDto> GetOverallLmsStatsAsync();

        // ADDED THIS
        Task<IEnumerable<DailyScreenTimeDto>> GetWeeklyScreenTimeAsync(string userId);
    }
}