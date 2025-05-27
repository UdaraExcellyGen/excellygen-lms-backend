using ExcellyGenLMS.Application.DTOs.Auth;
namespace ExcellyGenLMS.Application.Interfaces.Auth
{
    public interface IAuthService
    {
        Task<TokenDto> LoginAsync(LoginDto loginDto);
        Task<TokenDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
        Task<bool> RevokeTokenAsync(string refreshToken);
        Task<bool> ResetPasswordAsync(string email);
        Task<TokenDto> SelectRoleAsync(SelectRoleDto selectRoleDto);
        Task<bool> ValidateTokenAsync(string token);

        // Add this new method
        Task<bool> ChangePasswordAsync(ChangePasswordDto changePasswordDto);
    }
}