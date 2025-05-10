using Microsoft.Extensions.Configuration;
using Firebase.Auth;
using ExcellyGenLMS.Application.DTOs.Auth;
using ExcellyGenLMS.Application.Interfaces.Auth;
using ExcellyGenLMS.Core.Entities.Auth;
using ExcellyGenLMS.Core.Interfaces.Repositories.Auth;
using System.Security.Authentication;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace ExcellyGenLMS.Application.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IFirebaseAuthService _firebaseAuthService;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;

        public AuthService(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IFirebaseAuthService firebaseAuthService,
            ITokenService tokenService,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _firebaseAuthService = firebaseAuthService;
            _tokenService = tokenService;
            _configuration = configuration;
        }

        public async Task<TokenDto> LoginAsync(LoginDto loginDto)
        {
            try
            {
                Console.WriteLine($"Login attempt for: {loginDto.Email}");

                // First find the user in our database by email
                var user = await _userRepository.GetUserByEmailAsync(loginDto.Email);

                if (user == null)
                {
                    Console.WriteLine("User not found in database");
                    throw new AuthenticationException("User not found");
                }

                if (user.Status != "active")
                {
                    Console.WriteLine("User is inactive");
                    throw new AuthenticationException("User is inactive");
                }

                // WORKAROUND FOR AUTHENTICATION:
                // Instead of using Firebase Auth Client directly, we'll validate the password
                // using the Admin SDK's special method
                bool isAuthenticated = false;

                if (!string.IsNullOrEmpty(loginDto.Password))
                {
                    try
                    {
                        // If the user has a Firebase UID, use it to verify their credentials
                        if (!string.IsNullOrEmpty(user.FirebaseUid))
                        {
                            Console.WriteLine($"Verifying password for Firebase UID: {user.FirebaseUid}");

                            // Call AdminVerifyPassword method in FirebaseAuthService
                            isAuthenticated = await _firebaseAuthService.AdminVerifyPasswordAsync(loginDto.Email, loginDto.Password);
                            Console.WriteLine($"Admin SDK password verification result: {isAuthenticated}");

                            if (!isAuthenticated)
                            {
                                throw new AuthenticationException("Invalid password");
                            }
                        }
                        else
                        {
                            Console.WriteLine("User has no Firebase UID, cannot authenticate");
                            throw new AuthenticationException("User account not properly set up");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Authentication error: {ex.Message}");
                        throw new AuthenticationException($"Authentication failed: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("No password provided");
                    throw new AuthenticationException("Password is required");
                }

                // Determine the default role if the user has multiple roles
                Console.WriteLine("Finding default role");
                string defaultRole = user.Roles.FirstOrDefault() ?? throw new AuthenticationException("User has no roles");

                Console.WriteLine($"Default role: {defaultRole}");

                // Generate JWT tokens
                Console.WriteLine("Generating JWT tokens");
                var tokenDto = _tokenService.GenerateTokens(user, defaultRole);

                // Create and save refresh token
                Console.WriteLine("Creating refresh token");
                var refreshTokenExpiryDays = int.Parse(_configuration["Jwt:RefreshTokenExpiryInDays"] ?? "7");
                var refreshToken = new RefreshToken
                {
                    UserId = user.Id,
                    Token = tokenDto.RefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpiryDays)
                };

                await _refreshTokenRepository.CreateAsync(refreshToken);
                Console.WriteLine("Login successful");

                // Add password change requirement flag to token response
                tokenDto.RequirePasswordChange = user.RequirePasswordChange;

                return tokenDto;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw; // Rethrow to let the controller handle it
            }
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
        {
            try
            {
                Console.WriteLine($"Password change request for user: {changePasswordDto.UserId}");

                // Get the user
                var user = await _userRepository.GetUserByIdAsync(changePasswordDto.UserId);
                if (user == null)
                {
                    Console.WriteLine("User not found");
                    throw new InvalidOperationException("User not found");
                }

                // Use the same AdminVerifyPasswordAsync workaround as login
                bool passwordVerified = false;
                try
                {
                    // Verify current password using Admin SDK workaround
                    passwordVerified = await _firebaseAuthService.AdminVerifyPasswordAsync(
                        user.Email,
                        changePasswordDto.CurrentPassword
                    );

                    if (!passwordVerified)
                    {
                        Console.WriteLine("Current password verification failed");
                        throw new InvalidOperationException("Current password is incorrect");
                    }

                    Console.WriteLine("Current password verified successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Password verification error: {ex.Message}");
                    throw new InvalidOperationException("Current password is incorrect");
                }

                // Update password in Firebase
                try
                {
                    await _firebaseAuthService.UpdateUserPasswordAsync(
                        user.FirebaseUid,
                        changePasswordDto.NewPassword
                    );

                    // Update the password change requirement flag
                    user.RequirePasswordChange = false;
                    await _userRepository.UpdateUserAsync(user);

                    Console.WriteLine("Password changed successfully");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Password change error: {ex.Message}");
                    throw new InvalidOperationException($"Failed to change password: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Password change error: {ex.Message}");
                throw;
            }
        }

        public async Task<TokenDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            try
            {
                Console.WriteLine("Processing refresh token request");
                var principal = _tokenService.GetPrincipalFromExpiredToken(refreshTokenDto.AccessToken);
                var userId = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
                var currentRole = principal.Claims.FirstOrDefault(c => c.Type == "CurrentRole")?.Value;

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(currentRole))
                {
                    Console.WriteLine("Invalid access token: missing claims");
                    throw new SecurityTokenException("Invalid access token");
                }

                var refreshTokenEntity = await _refreshTokenRepository.GetByTokenAsync(refreshTokenDto.RefreshToken);

                if (refreshTokenEntity == null)
                {
                    Console.WriteLine("Invalid refresh token: token not found");
                    throw new SecurityTokenException("Invalid refresh token");
                }

                if (refreshTokenEntity.UserId != userId)
                {
                    Console.WriteLine("Refresh token does not match the user");
                    throw new SecurityTokenException("Refresh token does not match the user");
                }

                if (refreshTokenEntity.ExpiresAt < DateTime.UtcNow)
                {
                    Console.WriteLine("Refresh token has expired");
                    throw new SecurityTokenException("Refresh token has expired");
                }

                if (refreshTokenEntity.IsUsed || refreshTokenEntity.IsRevoked)
                {
                    Console.WriteLine("Refresh token has been used or revoked");
                    throw new SecurityTokenException("Refresh token has been used or revoked");
                }

                // Mark the current refresh token as used
                refreshTokenEntity.IsUsed = true;
                await _refreshTokenRepository.UpdateAsync(refreshTokenEntity);
                Console.WriteLine("Marked old refresh token as used");

                // Generate new tokens
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    Console.WriteLine("User not found");
                    throw new SecurityTokenException("User not found");
                }

                Console.WriteLine($"Generating new tokens for user: {user.Id}, role: {currentRole}");
                var newTokenDto = _tokenService.GenerateTokens(user, currentRole);

                // Create and save new refresh token
                var refreshTokenExpiryDays = int.Parse(_configuration["Jwt:RefreshTokenExpiryInDays"] ?? "7");
                var newRefreshToken = new RefreshToken
                {
                    UserId = userId,
                    Token = newTokenDto.RefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpiryDays)
                };

                await _refreshTokenRepository.CreateAsync(newRefreshToken);
                Console.WriteLine("Created new refresh token");

                // Add password change requirement flag to token response
                newTokenDto.RequirePasswordChange = user.RequirePasswordChange;

                return newTokenDto;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Refresh token error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            try
            {
                Console.WriteLine("Revoking refresh token");
                var refreshTokenEntity = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

                if (refreshTokenEntity == null)
                {
                    Console.WriteLine("Token not found");
                    return false;
                }

                refreshTokenEntity.IsRevoked = true;
                await _refreshTokenRepository.UpdateAsync(refreshTokenEntity);
                Console.WriteLine("Token revoked successfully");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Revoke token error: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> ResetPasswordAsync(string email)
        {
            try
            {
                Console.WriteLine($"Reset password request for email: {email}");
                var user = await _userRepository.GetUserByEmailAsync(email);

                if (user == null)
                {
                    // Don't reveal that the user doesn't exist for security reasons
                    Console.WriteLine("User not found, but not revealing for security");
                    return false;
                }

                var result = await _firebaseAuthService.ResetPasswordAsync(email);
                Console.WriteLine($"Password reset result: {result}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Reset password error: {ex.Message}");
                throw;
            }
        }

        public async Task<TokenDto> SelectRoleAsync(SelectRoleDto selectRoleDto)
        {
            try
            {
                Console.WriteLine($"Role selection request for user: {selectRoleDto.UserId}, role: {selectRoleDto.Role}");
                var user = await _userRepository.GetUserByIdAsync(selectRoleDto.UserId);

                if (user == null)
                {
                    Console.WriteLine("User not found");
                    throw new AuthenticationException("User not found");
                }

                if (user.Status != "active")
                {
                    Console.WriteLine("User is inactive");
                    throw new AuthenticationException("User is inactive");
                }

                if (!user.Roles.Contains(selectRoleDto.Role))
                {
                    Console.WriteLine("User does not have the selected role");
                    throw new AuthenticationException("User does not have the selected role");
                }

                // Generate new tokens for the selected role
                Console.WriteLine("Generating tokens for new role");
                var tokenDto = _tokenService.GenerateTokens(user, selectRoleDto.Role);

                // Create and save new refresh token
                Console.WriteLine("Creating refresh token");
                var refreshTokenExpiryDays = int.Parse(_configuration["Jwt:RefreshTokenExpiryInDays"] ?? "7");
                var refreshToken = new RefreshToken
                {
                    UserId = user.Id,
                    Token = tokenDto.RefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpiryDays)
                };

                await _refreshTokenRepository.CreateAsync(refreshToken);
                Console.WriteLine("Role selection successful");

                // Add password change requirement flag to token response
                tokenDto.RequirePasswordChange = user.RequirePasswordChange;

                return tokenDto;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Role selection error: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                Console.WriteLine("Validating token");
                var principal = _tokenService.GetPrincipalFromExpiredToken(token);
                var userId = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine("Invalid token: User ID claim not found");
                    return false;
                }

                // Check if user exists and is active
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    Console.WriteLine("User not found");
                    return false;
                }

                if (user.Status != "active")
                {
                    Console.WriteLine("User is inactive");
                    return false;
                }

                Console.WriteLine("Token is valid");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Token validation error: {ex.Message}");
                return false;
            }
        }
    }
}