using ExcellyGenLMS.Application.DTOs.Auth;
using ExcellyGenLMS.Core.Entities.Auth;
using System.Security.Claims;

namespace ExcellyGenLMS.Application.Interfaces.Auth
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user, string currentRole);
        string GenerateRefreshToken();
        TokenDto GenerateTokens(User user, string currentRole);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        string GetUserIdFromToken(string token);
        string GetCurrentRoleFromToken(string token);
    }
}