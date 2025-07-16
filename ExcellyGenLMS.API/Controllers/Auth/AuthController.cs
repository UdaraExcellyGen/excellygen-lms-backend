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
using System.Security.Claims;

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
        private readonly IUserManagementService? _userManagementService;

        public AuthController(
            IAuthService authService,
            IUserService userService,
            IUserRepository userRepository,
            ILogger<AuthController> logger,
            IFirebaseAuthService firebaseAuthService,
            IEmailService emailService,
            ITokenService tokenService,
            IUserManagementService? userManagementService = null)
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
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _firebaseAuthService.VerifyTokenAsync(loginDto.FirebaseToken);
                            _logger.LogInformation("Firebase token validated successfully");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Firebase token validation failed: {ex.Message}");
                        }
                    });
                }

                var tokenDto = await _authService.LoginAsync(loginDto);

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
            catch (AuthenticationException ex)
            {
                _logger.LogWarning($"Authentication failed: {ex.Message}");
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred during login" });
            }
        }

        [HttpPost("heartbeat")]
        [Authorize]
        public async Task<IActionResult> Heartbeat()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Invalid token: User identifier not found." });
                }
                var result = await _authService.HeartbeatAsync(userId);
                return Ok(result);
            }
            catch (AuthenticationException ex)
            {
                _logger.LogWarning($"Heartbeat authentication failed: {ex.Message}");
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Heartbeat error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred during heartbeat processing." });
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
                return Ok(new { message = "If your email exists in our system, a password reset link has been sent" });
            }
            catch (Exception ex)
            {
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

        [HttpPost("sync-firebase-users")]
        [Authorize(Roles = "Admin,SuperAdmin")]
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
                        if (string.IsNullOrEmpty(user.FirebaseUid) || user.FirebaseUid.Length < 20 || user.FirebaseUid.StartsWith("$2"))
                        {
                            _logger.LogInformation($"Syncing user {user.Email} with Firebase");
                            string tempPassword = Guid.NewGuid().ToString().Substring(0, 12) + "!A1";
                            string firebaseUid = await _firebaseAuthService.CreateUserAsync(new CreateUserDto
                            {
                                Email = user.Email,
                                Password = tempPassword,
                                Name = user.Name,
                                Phone = user.Phone ?? "",
                                Roles = user.Roles ?? new List<string>(),
                                Department = user.Department ?? ""
                            });
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
                var user = await _userService.GetUserByIdAsync(request.UserId);
                if (user == null || user.Email != request.Email)
                {
                    return Ok();
                }
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
        [Authorize(Roles = "Admin,SuperAdmin")]
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
                            if (role.Equals("admin", StringComparison.OrdinalIgnoreCase) && role != "Admin") { formattedRole = "Admin"; needsUpdate = true; }
                            else if (role.Equals("learner", StringComparison.OrdinalIgnoreCase) && role != "Learner") { formattedRole = "Learner"; needsUpdate = true; }
                            else if ((role.Equals("coordinator", StringComparison.OrdinalIgnoreCase) || role.Equals("course coordinator", StringComparison.OrdinalIgnoreCase) || role.Equals("course_coordinator", StringComparison.OrdinalIgnoreCase)) && role != "CourseCoordinator") { formattedRole = "CourseCoordinator"; needsUpdate = true; }
                            else if ((role.Equals("project_manager", StringComparison.OrdinalIgnoreCase) || role.Equals("project manager", StringComparison.OrdinalIgnoreCase)) && role != "ProjectManager") { formattedRole = "ProjectManager"; needsUpdate = true; }
                            else if (role.Equals("superadmin", StringComparison.OrdinalIgnoreCase) && role != "SuperAdmin") { formattedRole = "SuperAdmin"; needsUpdate = true; }
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
                return Ok(new { message = $"Fixed role capitalization for {fixedCount} users", fixedCount, totalUsers = users.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fixing role capitalization");
                return StatusCode(500, new { message = "An error occurred while fixing role capitalization", details = ex.Message });
            }
        }

        [HttpPost("promote-to-superadmin")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> PromoteToSuperAdmin([FromBody] PromoteToSuperAdminDto request)
        {
            try
            {
                _logger.LogInformation($"SuperAdmin promotion request for user: {request.UserId}");
                var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }
                if (_userManagementService == null)
                {
                    _logger.LogWarning("UserManagementService is not available");
                    return StatusCode(500, new { message = "SuperAdmin promotion service is not available" });
                }
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

    public class PromoteToSuperAdminDto
    {
        public required string UserId { get; set; }
    }
}