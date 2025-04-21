using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ExcellyGenLMS.Application.DTOs.Admin;
using ExcellyGenLMS.Application.DTOs.Auth;
using ExcellyGenLMS.Application.Interfaces.Admin;
using ExcellyGenLMS.Application.Interfaces.Auth;
using ExcellyGenLMS.Core.Entities.Auth;
using ExcellyGenLMS.Core.Interfaces.Repositories.Auth;
using Microsoft.Extensions.Logging;

namespace ExcellyGenLMS.Application.Services.Admin
{
    public class UserManagementService : IUserManagementService
    {
        private readonly IUserRepository _userRepository;
        private readonly IFirebaseAuthService _firebaseAuthService;
        private readonly ILogger<UserManagementService> _logger;

        // Snowflake ID variables
        private static readonly object _lockObject = new object();
        private static long _lastTimestamp = -1L;
        private static int _sequence = 0;
        private const long EPOCH = 1609459200000L; // January 1, 2021 as epoch

        public UserManagementService(
            IUserRepository userRepository,
            IFirebaseAuthService firebaseAuthService,
            ILogger<UserManagementService> logger)
        {
            _userRepository = userRepository;
            _firebaseAuthService = firebaseAuthService;
            _logger = logger;
        }

        public async Task<List<AdminUserDto>> GetAllUsersAsync()
        {
            try
            {
                var users = await _userRepository.GetAllUsersAsync();
                return users.Select(MapToAdminUserDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                throw;
            }
        }

        public async Task<AdminUserDto?> GetUserByIdAsync(string id)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null) return null;
                return MapToAdminUserDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user with ID: {id}");
                throw;
            }
        }

        public async Task<AdminUserDto> CreateUserAsync(AdminCreateUserDto createUserDto)
        {
            try
            {
                // Check if email is already in use
                var existingUser = await _userRepository.GetUserByEmailAsync(createUserDto.Email);
                if (existingUser != null)
                {
                    throw new InvalidOperationException($"Email {createUserDto.Email} is already in use");
                }

                // Format role names with proper capitalization
                createUserDto.Roles = FormatRoleNames(createUserDto.Roles);

                // Validate/Format the phone number
                string formattedPhone = FormatPhoneNumber(createUserDto.Phone);
                _logger.LogInformation($"Formatted phone number from '{createUserDto.Phone}' to '{formattedPhone}'");
                createUserDto.Phone = formattedPhone;

                // First create the user in Firebase
                string firebaseUid;
                try
                {
                    firebaseUid = await _firebaseAuthService.CreateUserAsync(new CreateUserDto
                    {
                        Email = createUserDto.Email,
                        Password = createUserDto.Password,
                        Name = createUserDto.Name,
                        Phone = createUserDto.Phone,
                        Roles = createUserDto.Roles,
                        Department = createUserDto.Department
                    });
                    _logger.LogInformation($"Firebase user created with UID: {firebaseUid}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create Firebase user");
                    throw new InvalidOperationException("Failed to create Firebase account: " + ex.Message);
                }

                // Generate a unique Snowflake ID instead of using Guid
                string userId = GenerateUniqueUserId();
                _logger.LogInformation($"Generated user ID: {userId}");

                var newUser = new User
                {
                    Id = userId,
                    Name = createUserDto.Name,
                    Email = createUserDto.Email,
                    Phone = createUserDto.Phone,
                    Roles = createUserDto.Roles,
                    Department = createUserDto.Department,
                    Status = "active", // New users are active by default
                    JoinedDate = DateTime.UtcNow,
                    FirebaseUid = firebaseUid, // Store the actual Firebase UID
                    Avatar = "/avatars/default.jpg" // Default avatar
                };

                var success = await _userRepository.AddUserAsync(newUser);
                if (!success)
                {
                    throw new InvalidOperationException("Failed to save user to database");
                }

                return MapToAdminUserDto(newUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                throw;
            }
        }

        public async Task<AdminUserDto?> UpdateUserAsync(string id, AdminUpdateUserDto updateUserDto)
        {
            try
            {
                var existingUser = await _userRepository.GetUserByIdAsync(id);
                if (existingUser == null) return null;

                // Format role names with proper capitalization
                updateUserDto.Roles = FormatRoleNames(updateUserDto.Roles);

                // Validate/Format the phone number
                string formattedPhone = FormatPhoneNumber(updateUserDto.Phone);
                _logger.LogInformation($"Formatted phone number from '{updateUserDto.Phone}' to '{formattedPhone}'");
                updateUserDto.Phone = formattedPhone;

                // Check if email is already in use by another user
                if (existingUser.Email != updateUserDto.Email)
                {
                    var userWithSameEmail = await _userRepository.GetUserByEmailAsync(updateUserDto.Email);
                    if (userWithSameEmail != null && userWithSameEmail.Id != id)
                    {
                        throw new InvalidOperationException($"Email {updateUserDto.Email} is already in use");
                    }
                }

                // Update user properties
                existingUser.Name = updateUserDto.Name;
                existingUser.Email = updateUserDto.Email;
                existingUser.Phone = updateUserDto.Phone;
                existingUser.Roles = updateUserDto.Roles;
                existingUser.Department = updateUserDto.Department;
                existingUser.Status = updateUserDto.Status;

                // Update user in Firebase if necessary
                if (!string.IsNullOrEmpty(existingUser.FirebaseUid) && existingUser.FirebaseUid.Length > 20)
                {
                    // User has a valid Firebase UID, update in Firebase
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
                    // User doesn't have a valid Firebase UID but password was provided
                    // Try to create a new Firebase user
                    try
                    {
                        string firebaseUid = await _firebaseAuthService.CreateUserAsync(new CreateUserDto
                        {
                            Email = updateUserDto.Email,
                            Password = updateUserDto.Password ?? "Temp123!",
                            Name = updateUserDto.Name,
                            Phone = updateUserDto.Phone ?? "",
                            Roles = updateUserDto.Roles,
                            Department = updateUserDto.Department ?? ""
                        });

                        existingUser.FirebaseUid = firebaseUid;
                        _logger.LogInformation($"Created Firebase user during update, UID: {firebaseUid}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to create Firebase user during update");
                    }
                }

                var success = await _userRepository.UpdateUserAsync(existingUser);
                if (!success)
                {
                    throw new InvalidOperationException("Failed to update user in database");
                }

                return MapToAdminUserDto(existingUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user with ID: {id}");
                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null) return false;

                // Delete from Firebase if user has a Firebase UID
                if (!string.IsNullOrEmpty(user.FirebaseUid) && user.FirebaseUid.Length > 20)
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

                return await _userRepository.DeleteUserAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting user with ID: {id}");
                throw;
            }
        }

        public async Task<AdminUserDto?> ToggleUserStatusAsync(string id)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null) return null;

                // Toggle status
                user.Status = user.Status == "active" ? "inactive" : "active";

                // Update Firebase user status
                if (!string.IsNullOrEmpty(user.FirebaseUid) && user.FirebaseUid.Length > 20)
                {
                    try
                    {
                        await _firebaseAuthService.SetUserDisabledStatusAsync(
                            user.FirebaseUid,
                            user.Status == "inactive"
                        );
                        _logger.LogInformation($"Firebase user status updated to {user.Status}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to update Firebase user status");
                    }
                }

                var success = await _userRepository.UpdateUserAsync(user);
                if (!success)
                {
                    throw new InvalidOperationException("Failed to update user status in database");
                }

                return MapToAdminUserDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error toggling status for user with ID: {id}");
                throw;
            }
        }

        public async Task<List<AdminUserDto>> SearchUsersAsync(AdminUserSearchParams searchParams)
        {
            try
            {
                var users = await _userRepository.SearchUsersAsync(
                    searchParams.SearchTerm,
                    searchParams.Roles,
                    searchParams.Status ?? "all"
                );

                return users.Select(MapToAdminUserDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users");
                throw;
            }
        }

        // Helper method to format phone numbers to E.164 format
        private string FormatPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return string.Empty;
            }

            // Remove any non-digit characters
            string digitsOnly = Regex.Replace(phoneNumber, @"\D", "");

            // If it already has a '+' prefix, return as is
            if (phoneNumber.StartsWith("+"))
            {
                return phoneNumber;
            }

            // Determine country code (default to +1 for US if not specified)
            // For international numbers, you might need more complex logic
            if (digitsOnly.Length == 10) // US number without country code
            {
                return "+1" + digitsOnly;
            }
            else if (digitsOnly.Length > 10) // Already has country code
            {
                return "+" + digitsOnly;
            }

            // If we can't format it properly, return empty string to avoid Firebase errors
            _logger.LogWarning($"Could not format phone number '{phoneNumber}' into E.164 format");
            return string.Empty;
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

        // Helper method to map User entity to AdminUserDto
        private AdminUserDto MapToAdminUserDto(User user)
        {
            return new AdminUserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Roles = user.Roles,
                Department = user.Department,
                Status = user.Status,
                JoinedDate = user.JoinedDate,
                JobRole = user.JobRole,
                About = user.About,
                Avatar = user.Avatar
            };
        }

        // Method to generate a unique Snowflake-inspired user ID
        private string GenerateUniqueUserId()
        {
            lock (_lockObject)
            {
                // Get current timestamp (milliseconds since epoch)
                long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                // Handle clock going backwards
                if (timestamp < _lastTimestamp)
                {
                    // Wait until we reach the previous timestamp
                    timestamp = _lastTimestamp;
                }

                // If we're in the same millisecond as the last ID
                if (timestamp == _lastTimestamp)
                {
                    // Increment sequence (0-4095)
                    _sequence = (_sequence + 1) & 0xFFF;

                    // If sequence overflow, wait for next millisecond
                    if (_sequence == 0)
                    {
                        while (timestamp <= _lastTimestamp)
                        {
                            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        }
                    }
                }
                else
                {
                    // New millisecond, reset sequence
                    _sequence = 0;
                }

                // Save the timestamp for next ID
                _lastTimestamp = timestamp;

                // Calculate 64-bit ID (combining timestamp, sequence, and a node ID of 1)
                // Cast the integer literals to long to avoid sign extension issues
                long id = ((timestamp - EPOCH) << 22) | ((long)1 << 12) | (long)_sequence;

                // Convert to Base36 for shorter, more readable IDs
                return $"USR-{ToBase36(id)}";
            }
        }

        // Helper method to convert long to Base36 string
        private string ToBase36(long value)
        {
            const string digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var result = "";

            // Special case for zero
            if (value == 0) return "0";

            // Convert to Base36
            while (value > 0)
            {
                result = digits[(int)(value % 36)] + result;
                value /= 36;
            }

            return result;
        }
    }
}