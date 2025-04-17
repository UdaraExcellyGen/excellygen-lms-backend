using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ExcellyGenLMS.Application.DTOs.Auth;
using ExcellyGenLMS.Application.Interfaces.Auth;
using ExcellyGenLMS.Core.Entities.Auth;
using System.Linq;

namespace ExcellyGenLMS.Infrastructure.Services.Auth
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateAccessToken(User user, string currentRole)
        {
            try
            {
                var issuer = _configuration["Jwt:Issuer"] ?? "ExcellyGenLMS";
                var audience = _configuration["Jwt:Audience"] ?? "ExcellyGenLMS.Client";
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"] ??
                    throw new InvalidOperationException("JWT Secret is not configured"));
                var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryInMinutes"] ?? "60");

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                        new Claim(JwtRegisteredClaimNames.Email, user.Email),
                        new Claim(JwtRegisteredClaimNames.Name, user.Name),
                        new Claim("FirebaseUid", user.FirebaseUid ?? string.Empty),
                        new Claim("CurrentRole", currentRole),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                    }),
                    Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                // Add all roles to claims
                if (user.Roles != null)
                {
                    foreach (var role in user.Roles)
                    {
                        tokenDescriptor.Subject.AddClaim(new Claim(ClaimTypes.Role, role));
                    }
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);

                Console.WriteLine($"Generated token with subject: {user.Id}");
                Console.WriteLine($"Token contains {tokenDescriptor.Subject.Claims.Count()} claims");

                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating access token: {ex.Message}");
                throw;
            }
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public TokenDto GenerateTokens(User user, string currentRole)
        {
            var accessToken = GenerateAccessToken(user, currentRole);
            var refreshToken = GenerateRefreshToken();
            var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryInMinutes"] ?? "60");

            return new TokenDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes),
                CurrentRole = currentRole
            };
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            try
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration["Jwt:Issuer"] ?? "ExcellyGenLMS",
                    ValidAudience = _configuration["Jwt:Audience"] ?? "ExcellyGenLMS.Client",
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"] ??
                        throw new InvalidOperationException("JWT Secret is not configured"))),
                    ValidateLifetime = false // Do not validate lifetime for refresh token check
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

                var jwtSecurityToken = securityToken as JwtSecurityToken;
                if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(
                    SecurityAlgorithms.HmacSha256Signature,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Invalid token");
                }

                return principal;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validating token: {ex.Message}");
                throw;
            }
        }

        public string GetUserIdFromToken(string token)
        {
            try
            {
                var principal = GetPrincipalFromExpiredToken(token);
                var claim = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);

                if (claim == null)
                {
                    Console.WriteLine("Subject claim not found in token");
                    return string.Empty;
                }

                Console.WriteLine($"Found user ID in token: {claim.Value}");
                return claim.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user ID from token: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return string.Empty;
            }
        }

        public string GetCurrentRoleFromToken(string token)
        {
            try
            {
                var principal = GetPrincipalFromExpiredToken(token);
                var claim = principal.Claims.FirstOrDefault(c => c.Type == "CurrentRole");

                if (claim == null)
                {
                    Console.WriteLine("Current role claim not found in token");
                    return string.Empty;
                }

                Console.WriteLine($"Found current role in token: {claim.Value}");
                return claim.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting current role from token: {ex.Message}");
                return string.Empty;
            }
        }
    }
}