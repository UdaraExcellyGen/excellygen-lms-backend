using System.Threading.Tasks;
using ExcellyGenLMS.Application.DTOs.Learner;

namespace ExcellyGenLMS.Application.Interfaces.Learner
{
    public interface ILeaderboardService
    {
        Task<LeaderboardDto> GetLeaderboardAsync(string currentUserId);
    }
}