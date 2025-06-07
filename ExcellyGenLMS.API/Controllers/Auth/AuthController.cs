using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ExcellyGenLMS.Application.DTOs.Auth;
using ExcellyGenLMS.Application.Interfaces.Auth;
using ExcellyGenLMS.Core.Interfaces.Repositories.Auth;
using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using ExcellyGenLMS.Application.Interfaces.Common;
using System.IdentityModel.Tokens.Jwt;
using ExcellyGenLMS.Application.Interfaces.Admin;
using System.Security;
using System.Linq;

namespace ExcellyGenLMS.API.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AuthController> _logger;
        private readonly IFirebaseAuthService _firebaseAuthService;
        private readonly IEmailService _emailService;
        private readonly ITokenService _tokenService;
        private readonly IUserManagementService? _userManagementService; // Made nullable

        public AuthController(
            IAuthService authService,
            IUserService userService,
            IUserRepository userRepository,
            ILogger<AuthController> logger,
            IFirebaseAuthService firebaseAuthService,
            IEmailService emailService,
            ITokenService tokenService,
            IUserManagementService? userManagementService = null) // Optional dependency to support existing code
        {
            _authService = authService;
            _userService = userService;
            _userRepository = userRepository;
            _logger = logger;
            _firebaseAuthService = firebaseAuthService;
            _emailService = emailService;
            _tokenService = tokenService;
            _userManagementService = userManagementService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResultDto>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                _logger.LogInformation($"Login attempt for email: {loginDto.Email}");

                if (!string.IsNullOrEmpty(loginDto.FirebaseToken))
                {
                    _logger.LogInformation("Firebase token provided");

                    // Verify the token with Firebase directly first
                    bool isValidToken = await _firebaseAuthService.VerifyTokenAsync(loginDto.FirebaseToken);
                    if (!isValidToken)
                    {
                        _logger.LogWarning("Firebase token validation failed");
                        return Unauthorized(new { message = "Invalid Firebase token" });
                    }

                    _logger.LogInformation("Firebase token validated successfully");
                }

                var tokenDto = await _authService.LoginAsync(loginDto);

                // Try-catch block to handle possible token extraction errors
                try
                {
                    var userId = _userService.GetUserIdFromToken(tokenDto.AccessToken);
                    _logger.LogInformation($"User ID extracted from token: {userId}");

                    var user = await _userRepository.GetUserByEmailAsync(loginDto.Email);

                    if (user == null)
                    {
                        _logger.LogWarning("User not found after login");
                        return Unauthorized(new { message = "User not found" });
                    }

                    var result = new AuthResultDto
                    {
                        UserId = user.Id,
                        Name = user.Name,
                        Email = user.Email,
                        Roles = user.Roles ?? new List<string>(),
                        Token = tokenDto,
                        RequirePasswordChange = user.RequirePasswordChange,
                        Avatar = user.Avatar
                    };

                    _logger.LogInformation($"Login successful for user: {user.Id}");
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error extracting user data from token: {ex.Message}");

                    // Fall back to querying by email
                    var user = await _userRepository.GetUserByEmailAsync(loginDto.Email);

                    if (user == null)
                    {
                        _logger.LogWarning("User not found after login");
                        return Unauthorized(new { message = "User not found" });
                    }

                    var result = new AuthResultDto
                    {
                        UserId = user.Id,
                        Name = user.Name,
                        Email = user.Email,
                        Roles = user.Roles ?? new List<string>(),
                        Token = tokenDto,
                        RequirePasswordChange = user.RequirePasswordChange,
                        Avatar = user.Avatar
                    };

                    _logger.LogInformation($"Login successful for user: {user.Id} (fallback method)");
                    return Ok(result);
                }
            }
            catch (AuthenticationException ex)
            {
                _logger.LogWarning($"Authentication failed: {ex.Message}");
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner exception: {ex.InnerException.Message}");
                }
                return StatusCode(500, new { message = "An error occurred during login", details = ex.Message });
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                _logger.LogInformation($"Password change request for user: {changePasswordDto.UserId}");
                var result = await _authService.ChangePasswordAsync(changePasswordDto);
                return Ok(new { success = result, message = "Password changed successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Password change failed: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Password change error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred during password change", details = ex.Message });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<TokenDto>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
                var tokenDto = await _authService.RefreshTokenAsync(refreshTokenDto);
                return Ok(tokenDto);
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning($"Token refresh failed: {ex.Message}");
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Token refresh error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred during token refresh", details = ex.Message });
            }
        }

        [HttpPost("revoke-token")]
        [Authorize]
        public async Task<ActionResult> RevokeToken([FromBody] string refreshToken)
        {
            try
            {
                var result = await _authService.RevokeTokenAsync(refreshToken);
                if (!result)
                {
                    return NotFound(new { message = "Token not found" });
                }

                return Ok(new { message = "Token revoked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Token revocation error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred", details = ex.Message });
            }
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            try
            {
                var result = await _authService.ResetPasswordAsync(resetPasswordDto.Email);

                // Always return success for security reasons
                return Ok(new { message = "If your email exists in our system, a password reset link has been sent" });
            }
            catch (Exception ex)
            {
                // Log the error but don't expose it to the client
                _logger.LogError($"Password reset error: {ex.Message}");
                return Ok(new { message = "If your email exists in our system, a password reset link has been sent" });
            }
        }

        [HttpPost("select-role")]
        [Authorize]
        public async Task<ActionResult<TokenDto>> SelectRole([FromBody] SelectRoleDto selectRoleDto)
        {
            try
            {
                var tokenDto = await _authService.SelectRoleAsync(selectRoleDto);
                return Ok(tokenDto);
            }
            catch (AuthenticationException ex)
            {
                _logger.LogWarning($"Role selection failed: {ex.Message}");
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Role selection error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred", details = ex.Message });
            }
        }

        [HttpGet("validate-token")]
        public async Task<ActionResult> ValidateToken([FromQuery] string token)
        {
            try
            {
                var isValid = await _authService.ValidateTokenAsync(token);
                return Ok(new { isValid });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Token validation error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred", details = ex.Message });
            }
        }

        [HttpPost("heartbeat")]
        public async Task<ActionResult> Heartbeat([FromBody] HeartbeatDto heartbeatDto)
        {
            try
            {
                _logger.LogInformation("Processing session heartbeat");

                if (string.IsNullOrEmpty(heartbeatDto.AccessToken))
                {
                    return BadRequest(new { message = "Access token is required" });
                }

                // Validate the token but also extend its lifetime
                var principal = _tokenService.GetPrincipalFromExpiredToken(heartbeatDto.AccessToken);
                var userId = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
                var currentRole = principal.Claims.FirstOrDefault(c => c.Type == "CurrentRole")?.Value;

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(currentRole))
                {
                    _logger.LogWarning("Invalid access token in heartbeat: missing claims");
                    return BadRequest(new { message = "Invalid access token" });
                }

                // Check if user exists and is active
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found in heartbeat");
                    return Unauthorized(new { message = "User not found" });
                }

                if (user.Status != "active")
                {
                    _logger.LogWarning("User is inactive in heartbeat");
                    return Unauthorized(new { message = "User is inactive" });
                }

                // Generate a new token with extended expiration
                var tokenDto = _tokenService.GenerateTokens(user, currentRole);

                // Update the expiry time on the client side
                return Ok(new
                {
                    accessToken = tokenDto.AccessToken,
                    expiresAt = tokenDto.ExpiresAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Heartbeat error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred", details = ex.Message });
            }
        }

        [HttpPost("sync-firebase-users")]
        [Authorize(Roles = "Admin,SuperAdmin")]  // Updated to include SuperAdmin
        public async Task<ActionResult> SyncFirebaseUsers()
        {
            try
            {
                _logger.LogInformation("Starting Firebase user synchronization");
                var users = await _userRepository.GetAllUsersAsync();
                int syncCount = 0;
                List<string> errors = new List<string>();

                foreach (var user in users)
                {
                    try
                    {
                        // Only sync users with missing or invalid Firebase UIDs
                        if (string.IsNullOrEmpty(user.FirebaseUid) || user.FirebaseUid.Length < 20 || user.FirebaseUid.StartsWith("$2"))
                        {
                            _logger.LogInformation($"Syncing user {user.Email} with Firebase");

                            // Generate a random password for existing users
                            string tempPassword = Guid.NewGuid().ToString().Substring(0, 12) + "!A1";

                            // Create or get user in Firebase
                            string firebaseUid = await _firebaseAuthService.CreateUserAsync(new CreateUserDto
                            {
                                Email = user.Email,
                                Password = tempPassword,
                                Name = user.Name,
                                Phone = user.Phone ?? "",
                                Roles = user.Roles ?? new List<string>(),
                                Department = user.Department ?? ""
                            });

                            // Update user in database with real Firebase UID
                            user.FirebaseUid = firebaseUid;
                            await _userRepository.UpdateUserAsync(user);

                            syncCount++;
                            _logger.LogInformation($"User {user.Email} synced with Firebase UID: {firebaseUid}");
                        }
                    }
                    catch (Exception ex)
                    {
                        string error = $"Error syncing user {user.Email}: {ex.Message}";
                        _logger.LogError(ex, error);
                        errors.Add(error);
                    }
                }

                return Ok(new
                {
                    message = $"Synced {syncCount} users with Firebase",
                    syncedCount = syncCount,
                    totalUsers = users.Count,
                    errors = errors.Count > 0 ? errors : null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing users with Firebase");
                return StatusCode(500, new { message = "An error occurred while syncing users", details = ex.Message });
            }
        }

        [HttpPost("send-temp-password")]
        public async Task<IActionResult> SendTemporaryPassword([FromBody] SendTempPasswordDto request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.TempPassword))
                {
                    return BadRequest("Missing required fields");
                }

                // Verify the user exists and matches the email
                var user = await _userService.GetUserByIdAsync(request.UserId);
                if (user == null || user.Email != request.Email)
                {
                    // Don't reveal specifics for security
                    return Ok(); // Return OK anyway to prevent user enumeration
                }

                // Send the email
                bool success = await _emailService.SendTemporaryPasswordEmailAsync(
                    user.Email,
                    user.Name,
                    request.TempPassword
                );

                if (!success)
                {
                    return StatusCode(500, "Failed to send email");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending temporary password email");
                return StatusCode(500, "An unexpected error occurred");
            }
        }

        [HttpPost("fix-role-capitalization")]
        [Authorize(Roles = "Admin,SuperAdmin")]  // Updated to include SuperAdmin
        public async Task<ActionResult> FixRoleCapitalization()
        {
            try
            {
                _logger.LogInformation("Starting role capitalization fix");
                var users = await _userRepository.GetAllUsersAsync();
                int fixedCount = 0;

                foreach (var user in users)
                {
                    bool needsUpdate = false;
                    var updatedRoles = new List<string>();

                    if (user.Roles != null)
                    {
                        foreach (var role in user.Roles)
                        {
                            string formattedRole = role;

                            // Apply proper capitalization based on your enum format
                            if (role.Equals("admin", StringComparison.OrdinalIgnoreCase) && role != "Admin")
                            {
                                formattedRole = "Admin";
                                needsUpdate = true;
                            }
                            else if (role.Equals("learner", StringComparison.OrdinalIgnoreCase) && role != "Learner")
                            {
                                formattedRole = "Learner";
                                needsUpdate = true;
                            }
                            else if ((role.Equals("coordinator", StringComparison.OrdinalIgnoreCase) ||
                                    role.Equals("course coordinator", StringComparison.OrdinalIgnoreCase) ||
                                    role.Equals("course_coordinator", StringComparison.OrdinalIgnoreCase)) &&
                                    role != "CourseCoordinator")
                            {
                                formattedRole = "CourseCoordinator";
                                needsUpdate = true;
                            }
                            else if ((role.Equals("project_manager", StringComparison.OrdinalIgnoreCase) ||
                                    role.Equals("project manager", StringComparison.OrdinalIgnoreCase)) &&
                                    role != "ProjectManager")
                            {
                                formattedRole = "ProjectManager";
                                needsUpdate = true;
                            }
                            else if (role.Equals("superadmin", StringComparison.OrdinalIgnoreCase) && role != "SuperAdmin")
                            {
                                formattedRole = "SuperAdmin";
                                needsUpdate = true;
                            }

                            updatedRoles.Add(formattedRole);
                        }
                    }

                    if (needsUpdate)
                    {
                        user.Roles = updatedRoles;
                        await _userRepository.UpdateUserAsync(user);
                        fixedCount++;
                        _logger.LogInformation($"Fixed role capitalization for user: {user.Email}");
                    }
                }

                return Ok(new
                {
                    message = $"Fixed role capitalization for {fixedCount} users",
                    fixedCount = fixedCount,
                    totalUsers = users.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fixing role capitalization");
                return StatusCode(500, new { message = "An error occurred while fixing role capitalization", details = ex.Message });
            }
        }

        // New endpoint for SuperAdmin promotion
        [HttpPost("promote-to-superadmin")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> PromoteToSuperAdmin([FromBody] PromoteToSuperAdminDto request)
        {
            try
            {
                _logger.LogInformation($"SuperAdmin promotion request for user: {request.UserId}");

                // Get the current user ID from the token
                var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                // Check if the UserManagementService is available
                if (_userManagementService == null)
                {
                    _logger.LogWarning("UserManagementService is not available");
                    return StatusCode(500, new { message = "SuperAdmin promotion service is not available" });
                }

                // Call the service method to promote the user
                try
                {
                    var result = await _userManagementService.PromoteToSuperAdminAsync(userId, request.UserId);
                    if (result == null)
                    {
                        return NotFound(new { message = "User not found" });
                    }

                    return Ok(result);
                }
                catch (SecurityException ex)
                {
                    _logger.LogWarning($"Permission denied: {ex.Message}");
                    return Forbid();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error promoting to SuperAdmin: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred", details = ex.Message });
            }
        }
    }

    // DTO for promoting a user to SuperAdmin
    public class PromoteToSuperAdminDto
    {
        public required string UserId { get; set; } // Added 'required' keyword to fix warning
    }
}