using ExcellyGenLMS.Core.Entities.Auth;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.Auth
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken> CreateAsync(RefreshToken refreshToken);
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task<List<RefreshToken>> GetAllActiveByUserIdAsync(string userId);
        Task<RefreshToken> UpdateAsync(RefreshToken refreshToken);
        Task RevokeAllUserTokensAsync(string userId);
    }
}