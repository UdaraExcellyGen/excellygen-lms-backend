using ExcellyGenLMS.Application.Interfaces.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Infrastructure.Services.Common
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<FileService> _logger;

        public FileService(
            IWebHostEnvironment webHostEnvironment,
            IHttpContextAccessor httpContextAccessor,
            ILogger<FileService> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
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

            var baseUrl = GetBaseUrl();

            return relativePath.StartsWith("http")
                ? relativePath
                : $"{baseUrl}{relativePath}";
        }

        public async Task<string> SaveFileAsync(IFormFile file, string subFolder)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("No file provided", nameof(file));
            }

            // Generate a unique file name
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", subFolder);

            // Create directory if it doesn't exist
            if (!Directory.Exists(_webHostEnvironment.WebRootPath))
            {
                Directory.CreateDirectory(_webHostEnvironment.WebRootPath);
                _logger.LogInformation("Created web root directory: {WebRootPath}", _webHostEnvironment.WebRootPath);
            }

            // Create uploads directory if it doesn't exist
            var uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
                _logger.LogInformation("Created uploads directory: {UploadsDir}", uploadsDir);
            }

            // Create sub-folder if it doesn't exist
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

            _logger.LogInformation("File saved: {FilePath}", filePath);

            return $"/uploads/{subFolder}/{fileName}";
        }

        public Task DeleteFileAsync(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath) || !relativePath.StartsWith("/uploads/"))
            {
                return Task.CompletedTask;
            }

            return Task.Run(() => {
                try
                {
                    var path = relativePath.TrimStart('/');
                    var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, path);

                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                        _logger.LogInformation("Deleted file: {FilePath}", fullPath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting file: {RelativePath}", relativePath);
                }
            });
        }
    }
}