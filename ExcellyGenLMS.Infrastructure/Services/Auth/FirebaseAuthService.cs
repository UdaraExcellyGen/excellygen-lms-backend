using Firebase.Auth;
using Firebase.Auth.Providers;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using ExcellyGenLMS.Application.DTOs.Auth;
using ExcellyGenLMS.Application.Interfaces.Auth;
using System;
using System.Threading.Tasks;
using System.IO;

namespace ExcellyGenLMS.Infrastructure.Services.Auth
{
    public class FirebaseAuthService : IFirebaseAuthService
    {
        private readonly FirebaseAuthClient _authClient;
        private readonly FirebaseAuth _firebaseAuth;
        private readonly IConfiguration _configuration;

        public FirebaseAuthService(IConfiguration configuration)
        {
            _configuration = configuration;

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
                Console.WriteLine("Firebase Auth Client initialized");

                // Firebase Admin SDK should already be initialized in Program.cs
                _firebaseAuth = FirebaseAuth.DefaultInstance;
                if (_firebaseAuth == null)
                {
                    throw new InvalidOperationException("FirebaseAuth.DefaultInstance is null. Make sure Firebase Admin SDK is initialized properly.");
                }
                Console.WriteLine("Firebase Auth instance obtained successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing FirebaseAuthService: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        public async Task<string> CreateUserAsync(CreateUserDto userDto)
        {
            try
            {
                Console.WriteLine($"Creating user with email: {userDto.Email}");
                // Create user with Firebase Admin SDK
                var userArgs = new UserRecordArgs
                {
                    Email = userDto.Email,
                    Password = userDto.Password,
                    DisplayName = userDto.Name,
                    PhoneNumber = userDto.Phone,
                    EmailVerified = true,
                    Disabled = false
                };

                var userRecord = await _firebaseAuth.CreateUserAsync(userArgs);
                Console.WriteLine($"User created with UID: {userRecord.Uid}");
                return userRecord.Uid;
            }
            catch (FirebaseAdmin.Auth.FirebaseAuthException ex)
            {
                Console.WriteLine($"Firebase authentication error: {ex.Message}");
                Console.WriteLine($"Error code: {ex.ErrorCode}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw new ApplicationException($"Firebase authentication error: {ex.Message}", ex);
            }
        }

        public async Task<bool> VerifyTokenAsync(string token)
        {
            try
            {
                Console.WriteLine("Starting token verification");
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("Token is null or empty");
                    return false;
                }

                Console.WriteLine($"Token length: {token.Length}");
                Console.WriteLine($"Token first 20 chars: {token.Substring(0, Math.Min(20, token.Length))}...");

                var decodedToken = await _firebaseAuth.VerifyIdTokenAsync(token);
                Console.WriteLine($"Token verified successfully. UID: {decodedToken.Uid}");
                return true;
            }
            catch (FirebaseAdmin.Auth.FirebaseAuthException ex)
            {
                Console.WriteLine($"Token verification failed: {ex.Message}");
                Console.WriteLine($"Error code: {ex.ErrorCode}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error verifying token: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        public async Task<string> GenerateCustomTokenAsync(string userId)
        {
            try
            {
                Console.WriteLine($"Generating custom token for user ID: {userId}");
                return await _firebaseAuth.CreateCustomTokenAsync(userId);
            }
            catch (FirebaseAdmin.Auth.FirebaseAuthException ex)
            {
                Console.WriteLine($"Failed to generate custom token: {ex.Message}");
                throw new ApplicationException($"Failed to generate custom token: {ex.Message}", ex);
            }
        }

        public async Task UpdateUserAsync(string firebaseUid, string email, string? password = null)
        {
            try
            {
                Console.WriteLine($"Updating user with Firebase UID: {firebaseUid}");
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
                Console.WriteLine("User updated successfully");
            }
            catch (FirebaseAdmin.Auth.FirebaseAuthException ex)
            {
                Console.WriteLine($"Failed to update user: {ex.Message}");
                throw new ApplicationException($"Failed to update user: {ex.Message}", ex);
            }
        }

        public async Task DeleteUserAsync(string firebaseUid)
        {
            try
            {
                Console.WriteLine($"Deleting user with Firebase UID: {firebaseUid}");
                await _firebaseAuth.DeleteUserAsync(firebaseUid);
                Console.WriteLine("User deleted successfully");
            }
            catch (FirebaseAdmin.Auth.FirebaseAuthException ex)
            {
                Console.WriteLine($"Failed to delete user: {ex.Message}");
                throw new ApplicationException($"Failed to delete user: {ex.Message}", ex);
            }
        }

        public async Task<bool> ResetPasswordAsync(string email)
        {
            try
            {
                Console.WriteLine($"Sending password reset email to: {email}");
                await _authClient.ResetEmailPasswordAsync(email);
                Console.WriteLine("Password reset email sent successfully");
                return true;
            }
            catch (Firebase.Auth.FirebaseAuthException ex)
            {
                // Log the error but don't expose details to the client
                Console.WriteLine($"Error resetting password: {ex.Message}");
                return false;
            }
        }

        public async Task<string> GetUserIdFromTokenAsync(string token)
        {
            try
            {
                Console.WriteLine("Getting user ID from token");
                var decodedToken = await _firebaseAuth.VerifyIdTokenAsync(token);
                Console.WriteLine($"User ID from token: {decodedToken.Uid}");
                return decodedToken.Uid;
            }
            catch (FirebaseAdmin.Auth.FirebaseAuthException ex)
            {
                Console.WriteLine($"Invalid token: {ex.Message}");
                throw new ApplicationException($"Invalid token: {ex.Message}", ex);
            }
        }
    }
}