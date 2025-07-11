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

            // If it's already a full HTTP URL, return as is
            if (relativePath.StartsWith("http"))
            {
                return relativePath;
            }

            // Handle legacy local paths - FIXED to handle both /uploads/ and /avatars/
            if (relativePath.StartsWith("/uploads/") || relativePath.StartsWith("/avatars/"))
            {
                _logger.LogWarning("Found legacy local path: {Path}. Converting to placeholder since wwwroot is disabled.", relativePath);

                // Check if it's a default avatar
                if (relativePath.Contains("default.jpg") || relativePath.Contains("default.png"))
                {
                    // Return a nice default avatar placeholder
                    return "https://ui-avatars.com/api/?name=User&background=BF4BF6&color=FFFFFF&size=150&rounded=true";
                }

                // For other legacy paths, return a placeholder with user initials
                return "https://ui-avatars.com/api/?name=User&background=52007C&color=FFFFFF&size=150&rounded=true";
            }

            // For relative Firebase paths (like "avatars/filename.jpg"), construct the full URL
            if (!relativePath.StartsWith("/"))
            {
                var encodedPath = Uri.EscapeDataString(relativePath);
                return $"https://firebasestorage.googleapis.com/v0/b/{_firebaseBucket}/o/{encodedPath}?alt=media";
            }

            // Fallback: return placeholder
            _logger.LogWarning("Unknown path format: {Path}. Using placeholder.", relativePath);
            return "https://ui-avatars.com/api/?name=User&background=7A00B8&color=FFFFFF&size=150&rounded=true";
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
                _logger.LogError(ex, "Error uploading file to Firebase Storage");
                throw; // Don't fallback to local storage since wwwroot is disabled
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
                    else if (relativePath.StartsWith("/uploads/") || relativePath.StartsWith("/avatars/"))
                    {
                        // Legacy local files - nothing to delete since wwwroot is disabled
                        _logger.LogInformation("Skipping deletion of legacy local file: {RelativePath}", relativePath);
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
    }
}