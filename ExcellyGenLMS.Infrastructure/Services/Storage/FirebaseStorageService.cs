using ExcellyGenLMS.Core.Interfaces.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Google.Cloud.Storage.V1;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace ExcellyGenLMS.Infrastructure.Services.Storage
{
    /// <summary>
    /// Implements IFileStorageService using Firebase Storage.
    /// </summary>
    public class FirebaseStorageService : IFileStorageService
    {
        private readonly ILogger<FirebaseStorageService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _firebaseProjectId;
        private readonly string _firebaseBucket;

        public FirebaseStorageService(
            ILogger<FirebaseStorageService> logger,
            IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            _firebaseProjectId = _configuration["Firebase:ProjectId"] ?? "excelly-lms-f3500";
            _firebaseBucket = _configuration["Firebase:StorageBucket"] ?? $"{_firebaseProjectId}.appspot.com";

            _logger.LogInformation("FirebaseStorageService initialized - ProjectId: {ProjectId}, Bucket: {Bucket}",
                _firebaseProjectId, _firebaseBucket);
        }

        public async Task<string?> SaveFileAsync(
            Stream fileStream,
            string fileName,
            string? contentType,
            string containerName,
            string[]? allowedExtensions = null,
            double maxFileSizeMB = 5)
        {
            if (fileStream == null || fileStream.Length == 0)
            {
                _logger.LogWarning("SaveFileAsync: Input stream is null or empty.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                _logger.LogWarning("SaveFileAsync: File name cannot be empty.");
                return null;
            }

            // Validate file extension
            var fileExtension = Path.GetExtension(fileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(fileExtension))
            {
                _logger.LogWarning("SaveFileAsync: File '{FileName}' has no extension.", fileName);
                return null;
            }

            if (allowedExtensions != null && allowedExtensions.Any() &&
                !allowedExtensions.Contains(fileExtension))
            {
                _logger.LogWarning("SaveFileAsync: File '{FileName}' has disallowed extension '{Extension}'. Allowed: {AllowedExtensions}",
                    fileName, fileExtension, string.Join(", ", allowedExtensions));
                return null;
            }

            // Validate file size
            if (fileStream.CanSeek && fileStream.Length > maxFileSizeMB * 1024 * 1024)
            {
                _logger.LogWarning("SaveFileAsync: File '{FileName}' exceeds size limit of {MaxSize}MB",
                    fileName, maxFileSizeMB);
                return null;
            }

            try
            {
                // Generate unique filename and path
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = $"{containerName.Replace("\\", "/")}/{uniqueFileName}";

                _logger.LogInformation("Uploading file '{FileName}' to Firebase Storage at path: {FilePath}",
                    fileName, filePath);

                // Set Firebase credentials
                var serviceAccountPath = _configuration["Firebase:ServiceAccountKeyPath"];
                if (!string.IsNullOrEmpty(serviceAccountPath) && File.Exists(serviceAccountPath))
                {
                    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", serviceAccountPath);
                }
                else
                {
                    _logger.LogError("Firebase service account file not found: {Path}", serviceAccountPath);
                    return null;
                }

                // Create storage client and upload
                var storage = StorageClient.Create();

                if (fileStream.CanSeek)
                {
                    fileStream.Position = 0;
                }

                var googleObject = await storage.UploadObjectAsync(
                    bucket: _firebaseBucket,
                    objectName: filePath,
                    contentType: contentType ?? "application/octet-stream",
                    source: fileStream,
                    options: new UploadObjectOptions
                    {
                        PredefinedAcl = PredefinedObjectAcl.PublicRead
                    });

                _logger.LogInformation("File uploaded successfully to Firebase Storage: {ObjectName}", googleObject.Name);

                // Return the relative path (not the full URL)
                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file '{FileName}' to Firebase Storage", fileName);
                return null;
            }
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                _logger.LogWarning("DeleteFileAsync: File path is null or empty.");
                return true; // Consider it successful if nothing to delete
            }

            try
            {
                // Set Firebase credentials
                var serviceAccountPath = _configuration["Firebase:ServiceAccountKeyPath"];
                if (!string.IsNullOrEmpty(serviceAccountPath) && File.Exists(serviceAccountPath))
                {
                    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", serviceAccountPath);
                }

                var storage = StorageClient.Create();
                await storage.DeleteObjectAsync(_firebaseBucket, filePath);

                _logger.LogInformation("Successfully deleted file from Firebase Storage: {FilePath}", filePath);
                return true;
            }
            catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("File not found in Firebase Storage (already deleted?): {FilePath}", filePath);
                return true; // Consider it successful if file doesn't exist
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file from Firebase Storage: {FilePath}", filePath);
                return false;
            }
        }

        public string GetFileUrl(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                _logger.LogWarning("GetFileUrl: File path is null or empty.");
                return string.Empty;
            }

            try
            {
                // Return the Firebase Storage public URL
                var encodedPath = Uri.EscapeDataString(filePath);
                var url = $"https://firebasestorage.googleapis.com/v0/b/{_firebaseBucket}/o/{encodedPath}?alt=media";

                return url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Firebase Storage URL for path: {FilePath}", filePath);
                return string.Empty;
            }
        }
    }
}