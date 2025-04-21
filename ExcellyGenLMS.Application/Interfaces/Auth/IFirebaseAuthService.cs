using ExcellyGenLMS.Application.DTOs.Auth;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Interfaces.Auth
{
    public interface IFirebaseAuthService
    {
        /// <summary>
        /// Creates a user in Firebase Authentication
        /// </summary>
        /// <param name="userDto">User data</param>
        /// <returns>Firebase UID of the created user</returns>
        Task<string> CreateUserAsync(CreateUserDto userDto);

        /// <summary>
        /// Verifies a Firebase ID token
        /// </summary>
        /// <param name="token">The Firebase ID token to verify</param>
        /// <returns>True if the token is valid, false otherwise</returns>
        Task<bool> VerifyTokenAsync(string token);

        /// <summary>
        /// Generates a custom Firebase token for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>Custom Firebase token</returns>
        Task<string> GenerateCustomTokenAsync(string userId);

        /// <summary>
        /// Updates a Firebase user's details
        /// </summary>
        /// <param name="firebaseUid">Firebase UID of the user to update</param>
        /// <param name="email">New email address</param>
        /// <param name="password">Optional new password</param>
        Task UpdateUserAsync(string firebaseUid, string email, string? password = null);

        /// <summary>
        /// Deletes a Firebase user
        /// </summary>
        /// <param name="firebaseUid">Firebase UID of the user to delete</param>
        Task DeleteUserAsync(string firebaseUid);

        /// <summary>
        /// Sends a password reset email to the specified address
        /// </summary>
        /// <param name="email">Email address to send the reset link to</param>
        /// <returns>True if email was sent successfully, false otherwise</returns>
        Task<bool> ResetPasswordAsync(string email);

        /// <summary>
        /// Extracts the Firebase UID from a verified token
        /// </summary>
        /// <param name="token">Firebase ID token</param>
        /// <returns>Firebase UID</returns>
        Task<string> GetUserIdFromTokenAsync(string token);

        /// <summary>
        /// Sets the disabled status of a Firebase user
        /// </summary>
        /// <param name="firebaseUid">Firebase UID of the user</param>
        /// <param name="disabled">True to disable the user, false to enable</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> SetUserDisabledStatusAsync(string firebaseUid, bool disabled);

        /// <summary>
        /// Syncs a user with Firebase, creating if they don't exist or updating if they do
        /// </summary>
        /// <param name="email">User's email</param>
        /// <param name="password">User's password</param>
        /// <returns>Firebase UID of the user</returns>
        Task<string> SyncUserWithFirebaseAsync(string email, string password);
    }
}