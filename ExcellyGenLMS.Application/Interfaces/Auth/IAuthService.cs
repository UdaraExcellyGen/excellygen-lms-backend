using ExcellyGenLMS.Application.DTOs.Auth;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Interfaces.Auth
{
    public interface IAuthService
    {
        Task<TokenDto> LoginAsync(LoginDto loginDto);
        Task<bool> ChangePasswordAsync(ChangePasswordDto changePasswordDto);
        Task<TokenDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
        Task<bool> RevokeTokenAsync(string refreshToken);
        Task<bool> ResetPasswordAsync(string email);
        Task<TokenDto> SelectRoleAsync(SelectRoleDto selectRoleDto);
        Task<bool> ValidateTokenAsync(string token);

        // ADDED THIS
        Task<HeartbeatDto> HeartbeatAsync(string userId);
    }
}