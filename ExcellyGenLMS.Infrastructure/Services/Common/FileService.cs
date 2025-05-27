using ExcellyGenLMS.Application.Interfaces.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Google.Cloud.Storage.V1;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace ExcellyGenLMS.Infrastructure.Services.Common
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<FileService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _firebaseProjectId;
        private readonly string _firebaseBucket;

        public FileService(
            IWebHostEnvironment webHostEnvironment,
            IHttpContextAccessor httpContextAccessor,
            ILogger<FileService> logger,
            IConfiguration configuration)
        {
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _configuration = configuration;

            // Get Firebase configuration
            _firebaseProjectId = _configuration["Firebase:ProjectId"] ?? "excelly-lms-f3500";
            _firebaseBucket = _configuration["Firebase:StorageBucket"] ?? $"{_firebaseProjectId}.appspot.com";

            _logger.LogInformation("FileService initialized - ProjectId: {ProjectId}, Bucket: {Bucket}", _firebaseProjectId, _firebaseBucket);
        }

        public string GetBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            return $"{request?.Scheme}://{request?.Host.Value}";
        }

        public string GetFullImageUrl(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                return string.Empty;
            }

            // If it's already a full Firebase Storage URL, return as is
            if (relativePath.StartsWith("https://firebasestorage.googleapis.com"))
            {
                return relativePath;
            }

            // If it's a legacy local path, convert to base URL
            if (relativePath.StartsWith("/uploads/"))
            {
                var baseUrl = GetBaseUrl();
                return $"{baseUrl}{relativePath}";
            }

            // For relative Firebase paths, construct the full URL
            return relativePath.StartsWith("http")
                ? relativePath
                : $"https://firebasestorage.googleapis.com/v0/b/{_firebaseBucket}/o/{Uri.EscapeDataString(relativePath)}?alt=media";
        }

        public async Task<string> SaveFileAsync(IFormFile file, string subFolder)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("No file provided", nameof(file));
            }

            try
            {
                // Try Firebase Storage first
                var firebaseUrl = await UploadToFirebaseStorageAsync(file, subFolder);
                _logger.LogInformation("File uploaded to Firebase Storage successfully: {FirebaseUrl}", firebaseUrl);
                return firebaseUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file to Firebase Storage, falling back to local storage");

                // Fallback to local storage if Firebase fails
                return await SaveFileLocallyAsync(file, subFolder);
            }
        }

        private async Task<string> UploadToFirebaseStorageAsync(IFormFile file, string subFolder)
        {
            try
            {
                _logger.LogInformation("Starting Firebase upload for file: {FileName}, Size: {Size} bytes", file.FileName, file.Length);

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = $"uploads/{subFolder}/{fileName}";

                _logger.LogInformation("Generated file path: {FilePath}", filePath);
                _logger.LogInformation("Using bucket: {Bucket}", _firebaseBucket);

                // Set environment variable for Google Cloud credentials
                var serviceAccountPath = _configuration["Firebase:ServiceAccountKeyPath"];
                if (!string.IsNullOrEmpty(serviceAccountPath) && File.Exists(serviceAccountPath))
                {
                    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", serviceAccountPath);
                    _logger.LogInformation("Set GOOGLE_APPLICATION_CREDENTIALS to: {Path}", serviceAccountPath);
                }
                else
                {
                    _logger.LogError("Service account file not found or not configured: {Path}", serviceAccountPath);
                    throw new FileNotFoundException($"Firebase service account file not found: {serviceAccountPath}");
                }

                // Create storage client
                var storage = StorageClient.Create();
                _logger.LogInformation("Created StorageClient successfully");

                // Upload file with public read access
                using var stream = file.OpenReadStream();

                _logger.LogInformation("Uploading to bucket: {Bucket}, object: {ObjectName}", _firebaseBucket, filePath);

                var googleObject = await storage.UploadObjectAsync(
                    bucket: _firebaseBucket,
                    objectName: filePath,
                    contentType: file.ContentType,
                    source: stream,
                    options: new UploadObjectOptions
                    {
                        PredefinedAcl = PredefinedObjectAcl.PublicRead
                    });

                _logger.LogInformation("Upload completed successfully. Object name: {ObjectName}", googleObject.Name);

                // Return the public URL
                var downloadUrl = $"https://firebasestorage.googleapis.com/v0/b/{_firebaseBucket}/o/{Uri.EscapeDataString(filePath)}?alt=media";
                _logger.LogInformation("Generated download URL: {DownloadUrl}", downloadUrl);

                return downloadUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Detailed error uploading to Firebase Storage. Bucket: {Bucket}, ProjectId: {ProjectId}, Error: {Error}",
                    _firebaseBucket, _firebaseProjectId, ex.Message);
                throw;
            }
        }

        private async Task<string> SaveFileLocallyAsync(IFormFile file, string subFolder)
        {
            _logger.LogWarning("Using local storage fallback for file: {FileName}", file.FileName);

            // Original local file saving logic as fallback
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", subFolder);

            // Create directory if it doesn't exist
            if (!Directory.Exists(_webHostEnvironment.WebRootPath))
            {
                Directory.CreateDirectory(_webHostEnvironment.WebRootPath);
                _logger.LogInformation("Created web root directory: {WebRootPath}", _webHostEnvironment.WebRootPath);
            }

            var uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
                _logger.LogInformation("Created uploads directory: {UploadsDir}", uploadsDir);
            }

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
                _logger.LogInformation("Created subdirectory: {UploadPath}", uploadPath);
            }

            var filePath = Path.Combine(uploadPath, fileName);

            // Save the file
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            _logger.LogInformation("File saved locally: {FilePath}", filePath);
            return $"/uploads/{subFolder}/{fileName}";
        }

        public Task DeleteFileAsync(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                return Task.CompletedTask;
            }

            return Task.Run(async () => {
                try
                {
                    // Check if it's a Firebase Storage URL
                    if (relativePath.StartsWith("https://firebasestorage.googleapis.com"))
                    {
                        await DeleteFromFirebaseStorageAsync(relativePath);
                    }
                    else if (relativePath.StartsWith("/uploads/"))
                    {
                        // Legacy local file deletion
                        await DeleteLocalFileAsync(relativePath);
                    }
                    else
                    {
                        _logger.LogWarning("Unknown file path format for deletion: {RelativePath}", relativePath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting file: {RelativePath}", relativePath);
                }
            });
        }

        private async Task DeleteFromFirebaseStorageAsync(string firebaseUrl)
        {
            try
            {
                // Extract object name from Firebase URL
                var uri = new Uri(firebaseUrl);
                var pathParts = uri.AbsolutePath.Split('/');

                // Find the object name after '/o/'
                var oIndex = Array.IndexOf(pathParts, "o");
                if (oIndex >= 0 && oIndex + 1 < pathParts.Length)
                {
                    var objectNameEncoded = pathParts[oIndex + 1];
                    var objectName = Uri.UnescapeDataString(objectNameEncoded);

                    // Set credentials if needed
                    var serviceAccountPath = _configuration["Firebase:ServiceAccountKeyPath"];
                    if (!string.IsNullOrEmpty(serviceAccountPath) && File.Exists(serviceAccountPath))
                    {
                        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", serviceAccountPath);
                    }

                    // Delete the object
                    var storage = StorageClient.Create();
                    await storage.DeleteObjectAsync(_firebaseBucket, objectName);

                    _logger.LogInformation("Successfully deleted file from Firebase Storage: {ObjectName}", objectName);
                }
                else
                {
                    throw new ArgumentException("Invalid Firebase Storage URL format");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting from Firebase Storage: {Url}", firebaseUrl);
                throw;
            }
        }

        private Task DeleteLocalFileAsync(string relativePath)
        {
            try
            {
                var path = relativePath.TrimStart('/');
                var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, path);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation("Deleted local file: {FilePath}", fullPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting local file: {RelativePath}", relativePath);
            }

            return Task.CompletedTask;
        }
    }
}