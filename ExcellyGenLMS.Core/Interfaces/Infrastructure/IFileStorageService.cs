// ExcellyGenLMS.Core/Interfaces/Infrastructure/IFileStorageService.cs
using System.IO; // Add this for Stream
using System.Threading.Tasks; // Add this for Task

namespace ExcellyGenLMS.Core.Interfaces.Infrastructure
{
    /// <summary>
    /// Defines operations for saving and deleting files in a storage system.
    /// (Modified to remove ASP.NET Core dependency)
    /// </summary>
    public interface IFileStorageService
    {
        /// <summary>
        /// Saves file content from a stream to storage within a specified container/directory.
        /// </summary>
        /// <param name="fileStream">The stream containing the file content.</param>
        /// <param name="fileName">The original name of the file, used for extension and context.</param>
        /// <param name="contentType">The MIME type of the file (e.g., "image/jpeg"). Optional but recommended.</param>
        /// <param name="containerName">A logical name for the container or directory (e.g., "thumbnails", "coursedocuments/lesson1").</param>
        /// <param name="allowedExtensions">Optional array of allowed file extensions (e.g., [".jpg", ".png"]). Case-insensitive. Null or empty array allows all.</param>
        /// <param name="maxFileSizeMB">Maximum allowed file size in megabytes (checked by the caller usually, but can be hint for service).</param>
        /// <returns>A Task representing the asynchronous operation. The task result contains the relative storage path of the saved file (e.g., "thumbnails/guid.jpg") or null if the save operation failed.</returns>
        Task<string?> SaveFileAsync(
            Stream fileStream,          // Changed from IFormFile
            string fileName,            // Added parameter
            string? contentType,         // Added parameter (optional)
            string containerName,
            string[]? allowedExtensions = null,
            double maxFileSizeMB = 5);

        /// <summary>
        /// Deletes a file from storage based on its relative path.
        /// </summary>
        /// <param name="filePath">The relative path to the file as stored (e.g., "thumbnails/guid.jpg").</param>
        /// <returns>A Task representing the asynchronous operation. The task result is true if the file was deleted successfully or did not exist; false if an error occurred during deletion.</returns>
        Task<bool> DeleteFileAsync(string filePath);

        /// <summary>
        /// Gets the publicly accessible URL for a given file path stored by the service.
        /// </summary>
        /// <param name="filePath">The relative path to the file as stored (e.g., "thumbnails/guid.jpg").</param>
        /// <returns>The full, publicly accessible URL (e.g., "https://yourdomain.com/uploads/thumbnails/guid.jpg") or an empty string/placeholder if the path is invalid.</returns>
        string GetFileUrl(string filePath);
    }
}