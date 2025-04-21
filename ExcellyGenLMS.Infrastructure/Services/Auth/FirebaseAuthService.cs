using Firebase.Auth;
using Firebase.Auth.Providers;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ExcellyGenLMS.Application.DTOs.Auth;
using ExcellyGenLMS.Application.Interfaces.Auth;
using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace ExcellyGenLMS.Infrastructure.Services.Auth
{
    public class FirebaseAuthService : IFirebaseAuthService
    {
        private readonly FirebaseAuthClient _authClient;
        private readonly FirebaseAuth _firebaseAuth;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FirebaseAuthService> _logger;

        public FirebaseAuthService(
            IConfiguration configuration,
            ILogger<FirebaseAuthService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            try
            {
                // Initialize Firebase Auth Client for client-side operations
                var config = new FirebaseAuthConfig
                {
                    ApiKey = configuration["Firebase:ApiKey"],
                    AuthDomain = configuration["Firebase:AuthDomain"],
                    Providers = new FirebaseAuthProvider[]
                    {
                        new EmailProvider()
                    }
                };
                _authClient = new FirebaseAuthClient(config);
                _logger.LogInformation("Firebase Auth Client initialized");

                // Firebase Admin SDK should already be initialized in Program.cs
                _firebaseAuth = FirebaseAuth.DefaultInstance;
                if (_firebaseAuth == null)
                {
                    throw new InvalidOperationException("FirebaseAuth.DefaultInstance is null. Make sure Firebase Admin SDK is initialized properly.");
                }
                _logger.LogInformation("Firebase Auth Admin instance obtained successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing FirebaseAuthService");
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, "Inner exception");
                }
                throw;
            }
        }

        public async Task<string> CreateUserAsync(CreateUserDto userDto)
        {
            try
            {
                _logger.LogInformation($"Creating Firebase user with email: {userDto.Email}");

                // Format the phone number properly or set to null if invalid
                string formattedPhone = FormatPhoneNumber(userDto.Phone);

                // Create user with Firebase Admin SDK
                var userArgs = new UserRecordArgs
                {
                    Email = userDto.Email,
                    Password = userDto.Password,
                    DisplayName = userDto.Name,
                    EmailVerified = true,
                    Disabled = false
                };

                // Only add phone number if it's properly formatted
                if (!string.IsNullOrEmpty(formattedPhone))
                {
                    userArgs.PhoneNumber = formattedPhone;
                }

                var userRecord = await _firebaseAuth.CreateUserAsync(userArgs);
                _logger.LogInformation($"Firebase user created with UID: {userRecord.Uid}");
                return userRecord.Uid;
            }
            catch (FirebaseAdmin.Auth.FirebaseAuthException ex)
            {
                _logger.LogWarning(ex, $"Firebase auth error: {ex.Message}");

                // If user already exists, try to get their UID
                if (ex.Message.Contains("email already exists"))
                {
                    try
                    {
                        var existingUser = await _firebaseAuth.GetUserByEmailAsync(userDto.Email);
                        _logger.LogInformation($"User already exists with UID: {existingUser.Uid}");

                        // Update password if provided
                        if (!string.IsNullOrEmpty(userDto.Password))
                        {
                            // Format phone number for update too
                            string formattedPhone = FormatPhoneNumber(userDto.Phone);

                            var updateArgs = new UserRecordArgs
                            {
                                Uid = existingUser.Uid,
                                Password = userDto.Password
                            };

                            // Only add phone if properly formatted
                            if (!string.IsNullOrEmpty(formattedPhone))
                            {
                                updateArgs.PhoneNumber = formattedPhone;
                            }

                            await _firebaseAuth.UpdateUserAsync(updateArgs);
                            _logger.LogInformation("User password updated");
                        }

                        return existingUser.Uid;
                    }
                    catch (Exception innerEx)
                    {
                        _logger.LogError(innerEx, "Error getting existing user from Firebase");
                        throw new ApplicationException($"Failed to get existing Firebase user: {innerEx.Message}", innerEx);
                    }
                }

                throw new ApplicationException($"Firebase authentication error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating Firebase user");
                throw new ApplicationException($"Failed to create Firebase user: {ex.Message}", ex);
            }
        }

        public async Task<bool> VerifyTokenAsync(string token)
        {
            try
            {
                _logger.LogInformation("Starting Firebase token verification");
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token is null or empty");
                    return false;
                }

                _logger.LogDebug($"Token length: {token.Length}");
                _logger.LogDebug($"Token first 20 chars: {token.Substring(0, Math.Min(20, token.Length))}...");

                var decodedToken = await _firebaseAuth.VerifyIdTokenAsync(token);
                _logger.LogInformation($"Token verified successfully. UID: {decodedToken.Uid}");
                return true;
            }
            catch (FirebaseAdmin.Auth.FirebaseAuthException ex)
            {
                _logger.LogWarning(ex, $"Token verification failed: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error verifying token");
                return false;
            }
        }

        public async Task<string> GenerateCustomTokenAsync(string userId)
        {
            try
            {
                _logger.LogInformation($"Generating custom token for user ID: {userId}");
                return await _firebaseAuth.CreateCustomTokenAsync(userId);
            }
            catch (FirebaseAdmin.Auth.FirebaseAuthException ex)
            {
                _logger.LogError(ex, "Failed to generate custom token");
                throw new ApplicationException($"Failed to generate custom token: {ex.Message}", ex);
            }
        }

        public async Task UpdateUserAsync(string firebaseUid, string email, string? password = null)
        {
            try
            {
                _logger.LogInformation($"Updating Firebase user with UID: {firebaseUid}");
                var args = new UserRecordArgs
                {
                    Uid = firebaseUid,
                    Email = email
                };

                if (!string.IsNullOrEmpty(password))
                {
                    args.Password = password;
                }

                await _firebaseAuth.UpdateUserAsync(args);
                _logger.LogInformation("Firebase user updated successfully");
            }
            catch (FirebaseAdmin.Auth.FirebaseAuthException ex)
            {
                _logger.LogError(ex, "Failed to update Firebase user");
                throw new ApplicationException($"Failed to update Firebase user: {ex.Message}", ex);
            }
        }

        public async Task DeleteUserAsync(string firebaseUid)
        {
            try
            {
                _logger.LogInformation($"Deleting Firebase user with UID: {firebaseUid}");
                await _firebaseAuth.DeleteUserAsync(firebaseUid);
                _logger.LogInformation("Firebase user deleted successfully");
            }
            catch (FirebaseAdmin.Auth.FirebaseAuthException ex)
            {
                _logger.LogError(ex, "Failed to delete Firebase user");
                throw new ApplicationException($"Failed to delete Firebase user: {ex.Message}", ex);
            }
        }

        public async Task<bool> ResetPasswordAsync(string email)
        {
            try
            {
                _logger.LogInformation($"Sending password reset email to: {email}");
                await _authClient.ResetEmailPasswordAsync(email);
                _logger.LogInformation("Password reset email sent successfully");
                return true;
            }
            catch (Firebase.Auth.FirebaseAuthException ex)
            {
                // Log the error but don't expose details to the client
                _logger.LogWarning(ex, "Error resetting Firebase password");
                return false;
            }
        }

        public async Task<string> GetUserIdFromTokenAsync(string token)
        {
            try
            {
                _logger.LogInformation("Getting user ID from Firebase token");
                var decodedToken = await _firebaseAuth.VerifyIdTokenAsync(token);
                _logger.LogInformation($"User ID from token: {decodedToken.Uid}");
                return decodedToken.Uid;
            }
            catch (FirebaseAdmin.Auth.FirebaseAuthException ex)
            {
                _logger.LogError(ex, "Invalid Firebase token");
                throw new ApplicationException($"Invalid Firebase token: {ex.Message}", ex);
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

        public async Task<bool> SetUserDisabledStatusAsync(string firebaseUid, bool disabled)
        {
            try
            {
                _logger.LogInformation($"Setting Firebase user {firebaseUid} disabled status to: {disabled}");

                await _firebaseAuth.UpdateUserAsync(new UserRecordArgs
                {
                    Uid = firebaseUid,
                    Disabled = disabled
                });

                _logger.LogInformation($"Firebase user disabled status updated successfully");
                return true;
            }
            catch (FirebaseAdmin.Auth.FirebaseAuthException ex)
            {
                _logger.LogError(ex, "Failed to update Firebase user disabled status");
                return false;
            }
        }

        public async Task<string> SyncUserWithFirebaseAsync(string email, string password)
        {
            try
            {
                _logger.LogInformation($"Syncing user with Firebase: {email}");

                // Try to get existing user
                try
                {
                    var existingUser = await _firebaseAuth.GetUserByEmailAsync(email);
                    _logger.LogInformation($"Found existing Firebase user with UID: {existingUser.Uid}");

                    // Update password if provided
                    if (!string.IsNullOrEmpty(password))
                    {
                        await _firebaseAuth.UpdateUserAsync(new UserRecordArgs
                        {
                            Uid = existingUser.Uid,
                            Password = password
                        });
                        _logger.LogInformation("Firebase user password updated");
                    }

                    return existingUser.Uid;
                }
                catch (FirebaseAdmin.Auth.FirebaseAuthException ex)
                {
                    // Check error message to see if user doesn't exist
                    if (ex.Message.Contains("user does not exist") || ex.Message.Contains("no user record"))
                    {
                        // User doesn't exist, create a new one
                        _logger.LogInformation("User not found in Firebase, creating new user");
                        var userArgs = new UserRecordArgs
                        {
                            Email = email,
                            Password = password,
                            EmailVerified = true,
                            Disabled = false
                        };

                        var userRecord = await _firebaseAuth.CreateUserAsync(userArgs);
                        _logger.LogInformation($"Firebase user created with UID: {userRecord.Uid}");
                        return userRecord.Uid;
                    }

                    // If it's not a "user not found" error, rethrow
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing user with Firebase");
                throw new ApplicationException($"Failed to sync user with Firebase: {ex.Message}", ex);
            }
        }
    }
}