using ExcellyGenLMS.Application.Interfaces.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Google.Cloud.Storage.V1;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ExcellyGenLMS.API.Controllers.Learner
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly ILogger<FilesController> _logger;
        private readonly IConfiguration _configuration;

        public FilesController(
            IFileService fileService,
            ILogger<FilesController> logger,
            IConfiguration configuration)
        {
            _fileService = fileService;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("upload/forum")]
        public async Task<IActionResult> UploadForumImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded or file is empty." });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !allowedExtensions.Contains(ext))
            {
                return BadRequest(new { message = "Invalid file type. Only JPG, JPEG, PNG, GIF are allowed." });
            }

            if (file.Length > 5 * 1024 * 1024) // 5MB limit
            {
                return BadRequest(new { message = "File size exceeds the 5MB limit." });
            }

            try
            {
                _logger.LogInformation("Starting forum image upload for file: {FileName}", file.FileName);

                var imageUrl = await _fileService.SaveFileAsync(file, "forum");
                var fullImageUrl = _fileService.GetFullImageUrl(imageUrl);

                _logger.LogInformation("Forum image uploaded successfully. ImageUrl: {ImageUrl}", fullImageUrl);

                return Ok(new
                {
                    imageUrl = fullImageUrl,
                    relativePath = imageUrl.StartsWith("https://") ? null : imageUrl
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("File upload argument error: {ErrorMessage}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading forum image.");
                return StatusCode(500, new { message = "An internal error occurred while uploading the file." });
            }
        }

        [HttpPost("upload/avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded or file is empty." });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !allowedExtensions.Contains(ext))
            {
                return BadRequest(new { message = "Invalid file type. Only JPG, JPEG, PNG, GIF are allowed." });
            }

            if (file.Length > 5 * 1024 * 1024) // 5MB limit
            {
                return BadRequest(new { message = "File size exceeds the 5MB limit." });
            }

            try
            {
                _logger.LogInformation("Starting avatar upload for file: {FileName}", file.FileName);

                var imageUrl = await _fileService.SaveFileAsync(file, "avatars");
                var fullImageUrl = _fileService.GetFullImageUrl(imageUrl);

                _logger.LogInformation("Avatar uploaded successfully. ImageUrl: {ImageUrl}", fullImageUrl);

                return Ok(new
                {
                    imageUrl = fullImageUrl,
                    avatar = fullImageUrl
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Avatar upload argument error: {ErrorMessage}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading avatar.");
                return StatusCode(500, new { message = "An internal error occurred while uploading the file." });
            }
        }

        [HttpGet("test-firebase")]
        [AllowAnonymous]
        public async Task<IActionResult> TestFirebaseConnection()
        {
            try
            {
                var projectId = _configuration["Firebase:ProjectId"];
                var bucketName = _configuration["Firebase:StorageBucket"];
                var serviceAccountPath = _configuration["Firebase:ServiceAccountKeyPath"];

                _logger.LogInformation("Testing Firebase connection...");
                _logger.LogInformation("Project ID: {ProjectId}", projectId);
                _logger.LogInformation("Bucket: {BucketName}", bucketName);
                _logger.LogInformation("Service Account Path: {ServiceAccountPath}", serviceAccountPath);

                // Use System.IO.File explicitly to avoid conflicts with ControllerBase.File
                var fileExists = !string.IsNullOrEmpty(serviceAccountPath) && System.IO.File.Exists(serviceAccountPath);

                if (!fileExists)
                {
                    return BadRequest(new
                    {
                        message = "Service account file not found or not configured",
                        path = serviceAccountPath,
                        fileExists = fileExists
                    });
                }

                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", serviceAccountPath);

                var storage = StorageClient.Create();
                var bucket = await storage.GetBucketAsync(bucketName);

                return Ok(new
                {
                    message = "Firebase Storage connection successful!",
                    projectId = projectId,
                    bucketName = bucketName,
                    bucketLocation = bucket.Location,
                    serviceAccountPath = serviceAccountPath,
                    serviceAccountFileExists = fileExists,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Firebase connection test failed");
                return StatusCode(500, new
                {
                    message = "Firebase connection failed",
                    error = ex.Message,
                    innerException = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteFile([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return BadRequest(new { message = "File path is required." });
            }

            if (!path.StartsWith("https://firebasestorage.googleapis.com") && !path.StartsWith("/uploads/"))
            {
                return BadRequest(new { message = "Invalid file path format for deletion." });
            }

            try
            {
                await _fileService.DeleteFileAsync(path);
                _logger.LogInformation("File deleted successfully: {Path}", path);
                return Ok(new { message = "File deleted successfully." });
            }
            catch (FileNotFoundException)
            {
                _logger.LogWarning("File not found for deletion: {Path}", path);
                return NotFound(new { message = "File not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {Path}", path);
                return StatusCode(500, new { message = "An error occurred while deleting the file." });
            }
        }
    }
}