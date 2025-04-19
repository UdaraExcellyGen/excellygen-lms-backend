using ExcellyGenLMS.Application.DTOs.Auth;

namespace ExcellyGenLMS.Application.Interfaces.Auth
{
    public interface IFirebaseAuthService
    {
        Task<string> CreateUserAsync(CreateUserDto userDto);
        Task<bool> VerifyTokenAsync(string token);
        Task<string> GenerateCustomTokenAsync(string userId);
        Task UpdateUserAsync(string firebaseUid, string email, string? password = null);
        Task DeleteUserAsync(string firebaseUid);
        Task<bool> ResetPasswordAsync(string email);
        Task<string> GetUserIdFromTokenAsync(string token);
    }
}