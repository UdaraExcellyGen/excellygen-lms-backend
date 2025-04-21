using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ExcellyGenLMS.Application.DTOs.Auth;
using ExcellyGenLMS.Application.Interfaces.Auth;
using ExcellyGenLMS.Core.Entities.Auth;
using ExcellyGenLMS.Core.Interfaces.Repositories.Auth;

namespace ExcellyGenLMS.Application.Services.Auth
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IFirebaseAuthService _firebaseAuthService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository userRepository,
            IPasswordHasher<User> passwordHasher,
            IFirebaseAuthService firebaseAuthService,
            IConfiguration configuration,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _firebaseAuthService = firebaseAuthService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            try
            {
                // Check if email is already in use
                var existingUser = await _userRepository.GetUserByEmailAsync(createUserDto.Email);
                if (existingUser != null)
                {
                    throw new InvalidOperationException($"Email {createUserDto.Email} is already in use");
                }

                _logger.LogInformation($"Creating user: {createUserDto.Email}");

                // Format role names with proper capitalization
                createUserDto.Roles = FormatRoleNames(createUserDto.Roles);

                // Create user in Firebase first to get Firebase UID
                string firebaseUid;
                try
                {
                    firebaseUid = await _firebaseAuthService.CreateUserAsync(createUserDto);
                    _logger.LogInformation($"Firebase user created with UID: {firebaseUid}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create Firebase user");
                    throw new InvalidOperationException("Failed to create Firebase user: " + ex.Message, ex);
                }

                var newUser = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = createUserDto.Name,
                    Email = createUserDto.Email,
                    Phone = createUserDto.Phone,
                    Roles = createUserDto.Roles,
                    Department = createUserDto.Department,
                    Status = "active", // New users are active by default
                    JoinedDate = DateTime.UtcNow,
                    FirebaseUid = firebaseUid // Store the actual Firebase UID
                };

                await _userRepository.AddUserAsync(newUser);
                _logger.LogInformation($"User created in database with ID: {newUser.Id}");
                return MapToUserDto(newUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning($"User with ID {id} not found for deletion");
                    return false;
                }

                _logger.LogInformation($"Deleting user with ID: {id}");

                // Delete from Firebase if user has a Firebase UID
                if (!string.IsNullOrEmpty(user.FirebaseUid))
                {
                    try
                    {
                        await _firebaseAuthService.DeleteUserAsync(user.FirebaseUid);
                        _logger.LogInformation($"Firebase user deleted: {user.FirebaseUid}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Failed to delete Firebase user: {user.FirebaseUid}");
                        // Continue with database deletion even if Firebase deletion fails
                    }
                }

                // Delete from database
                return await _userRepository.DeleteUserAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting user with ID: {id}");
                throw;
            }
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            try
            {
                var users = await _userRepository.GetAllUsersAsync();
                return users.Select(MapToUserDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                throw;
            }
        }

        public async Task<UserDto> GetUserByEmailAsync(string email)
        {
            try
            {
                var user = await _userRepository.GetUserByEmailAsync(email);
                if (user == null)
                {
                    throw new KeyNotFoundException($"User with email {email} not found");
                }
                return MapToUserDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user with email: {email}");
                throw;
            }
        }

        public async Task<UserDto> GetUserByIdAsync(string id)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                {
                    throw new KeyNotFoundException($"User with ID {id} not found");
                }
                return MapToUserDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user with ID: {id}");
                throw;
            }
        }

        public async Task<UserDto> GetUserFromTokenAsync(string token)
        {
            try
            {
                string userId = GetUserIdFromToken(token);
                if (string.IsNullOrEmpty(userId))
                {
                    throw new InvalidOperationException("Could not extract user ID from token");
                }

                return await GetUserByIdAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user from token");
                throw;
            }
        }

        public string GetUserIdFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                // Look for the "sub" claim which typically contains the user ID
                var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "userId" || c.Type == JwtRegisteredClaimNames.Sub);

                if (subClaim != null)
                {
                    return subClaim.Value;
                }

                // If "sub" claim is not found, try to find "nameid" which is sometimes used for user ID
                var nameIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid" || c.Type == JwtRegisteredClaimNames.NameId);

                if (nameIdClaim != null)
                {
                    return nameIdClaim.Value;
                }

                // If no standard claim is found, look for a custom claim that might contain the user ID
                var customIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "id" || c.Type.EndsWith("id", StringComparison.OrdinalIgnoreCase));

                return customIdClaim?.Value ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting user ID from token");
                return string.Empty;
            }
        }

        public async Task<UserDto> UpdateUserAsync(string id, UpdateUserDto updateUserDto)
        {
            try
            {
                var existingUser = await _userRepository.GetUserByIdAsync(id);
                if (existingUser == null)
                {
                    throw new KeyNotFoundException($"User with ID {id} not found");
                }

                // Format role names with proper capitalization
                updateUserDto.Roles = FormatRoleNames(updateUserDto.Roles);

                // Check if email is already in use by another user
                if (existingUser.Email != updateUserDto.Email)
                {
                    var userWithSameEmail = await _userRepository.GetUserByEmailAsync(updateUserDto.Email);
                    if (userWithSameEmail != null && userWithSameEmail.Id != id)
                    {
                        throw new InvalidOperationException($"Email {updateUserDto.Email} is already in use");
                    }
                }

                _logger.LogInformation($"Updating user with ID: {id}");

                // Update user in Firebase if needed
                if (!string.IsNullOrEmpty(existingUser.FirebaseUid))
                {
                    try
                    {
                        await _firebaseAuthService.UpdateUserAsync(
                            existingUser.FirebaseUid,
                            updateUserDto.Email,
                            updateUserDto.Password
                        );
                        _logger.LogInformation($"Firebase user updated: {existingUser.FirebaseUid}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Failed to update Firebase user: {existingUser.FirebaseUid}");
                    }
                }
                else if (!string.IsNullOrEmpty(updateUserDto.Password))
                {
                    // If no Firebase UID but password provided, try to create or sync with Firebase
                    try
                    {
                        var firebaseUid = await _firebaseAuthService.SyncUserWithFirebaseAsync(
                            updateUserDto.Email,
                            updateUserDto.Password
                        );

                        existingUser.FirebaseUid = firebaseUid;
                        _logger.LogInformation($"User synced with Firebase, UID: {firebaseUid}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to sync user with Firebase during update");
                    }
                }

                // Update user properties
                existingUser.Name = updateUserDto.Name;
                existingUser.Email = updateUserDto.Email;
                existingUser.Phone = updateUserDto.Phone;
                existingUser.Roles = updateUserDto.Roles;
                existingUser.Department = updateUserDto.Department;
                existingUser.Status = updateUserDto.Status;

                await _userRepository.UpdateUserAsync(existingUser);
                _logger.LogInformation($"User updated in database");

                return MapToUserDto(existingUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user with ID: {id}");
                throw;
            }
        }

        public async Task<UserDto> VerifyCredentialsAsync(string email, string password)
        {
            try
            {
                _logger.LogInformation($"Verifying credentials for email: {email}");

                var user = await _userRepository.GetUserByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning($"User with email {email} not found");
                    throw new InvalidOperationException("Invalid email or password");
                }

                // First, try to verify with Firebase if user has a Firebase UID
                if (!string.IsNullOrEmpty(user.FirebaseUid) && user.FirebaseUid.Length > 20)
                {
                    try
                    {
                        // For Firebase auth verification, we'd need to sign in with Firebase SDK
                        // This isn't directly possible from backend, so we rely on Firebase token validation
                        // in AuthService.LoginAsync
                        _logger.LogInformation("User has Firebase UID, deferring verification to Firebase");
                        return MapToUserDto(user);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during Firebase credential verification");
                        // Fall back to local verification if Firebase fails
                    }
                }

                // If the user has no Firebase UID or Firebase verification failed, and FirebaseUid has a BCrypt hash, 
                // verify password using BCrypt
                if (user.FirebaseUid != null && user.FirebaseUid.StartsWith("$2"))
                {
                    bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.FirebaseUid);
                    if (!isPasswordValid)
                    {
                        _logger.LogWarning("Invalid password for local verification");
                        throw new InvalidOperationException("Invalid email or password");
                    }

                    // Password verified locally, try to sync with Firebase
                    try
                    {
                        var firebaseUid = await _firebaseAuthService.SyncUserWithFirebaseAsync(email, password);
                        user.FirebaseUid = firebaseUid;
                        await _userRepository.UpdateUserAsync(user);
                        _logger.LogInformation($"User synced with Firebase during login, UID: {firebaseUid}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to sync with Firebase during login");
                        // Continue even if sync fails
                    }

                    return MapToUserDto(user);
                }

                // If we reach here, we have no way to verify the password
                _logger.LogWarning("No valid method to verify password");
                throw new InvalidOperationException("Invalid email or password");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error verifying credentials for user with email: {email}");
                throw;
            }
        }

        // Helper method to map User entity to UserDto
        private UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Roles = user.Roles ?? new List<string>(),
                Department = user.Department,
                Status = user.Status,
                JoinedDate = user.JoinedDate,
                JobRole = user.JobRole,
                About = user.About,
                FirebaseUid = user.FirebaseUid,
                Avatar = user.Avatar
            };
        }

        // Helper method to format role names for proper capitalization
        private List<string> FormatRoleNames(List<string> roles)
        {
            return roles.Select(role =>
            {
                // Skip if already properly formatted
                if (IsProperlyFormatted(role))
                {
                    return role;
                }

                // Convert "project_manager" to "ProjectManager"
                if (role.Contains("_"))
                {
                    return string.Join("", role.Split('_')
                        .Select(part => char.ToUpper(part[0]) + part.Substring(1).ToLower()));
                }

                // Special case for "coordinator" -> "CourseCoordinator"
                if (role.Equals("coordinator", StringComparison.OrdinalIgnoreCase))
                {
                    return "CourseCoordinator";
                }

                // Simple case like "admin" -> "Admin"
                return char.ToUpper(role[0]) + role.Substring(1).ToLower();
            }).ToList();
        }

        // Check if a role name is already properly formatted
        private bool IsProperlyFormatted(string role)
        {
            string[] validFormats = { "Admin", "Learner", "CourseCoordinator", "ProjectManager" };
            return validFormats.Contains(role);
        }
    }
}