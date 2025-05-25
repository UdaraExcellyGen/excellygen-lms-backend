// ExcellyGenLMS.Application/Interfaces/Learner/ILearnerStatsService.cs
using ExcellyGenLMS.Application.DTOs; // For OverallLmsStatsDto
using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Interfaces.Learner
{
    public interface ILearnerStatsService
    {
        Task<OverallLmsStatsDto> GetOverallLmsStatsAsync();
    }
}