using ExcellyGenLMS.Core.Interfaces.Infrastructure; // Core interface
using Microsoft.AspNetCore.Hosting;               // For IWebHostEnvironment (gives ContentRootPath, WebRootPath)
using Microsoft.AspNetCore.Http;                // For IFormFile (used in constructor context), IHttpContextAccessor
using Microsoft.Extensions.Configuration;       // For IConfiguration (to read appsettings)
using Microsoft.Extensions.Logging;             // For ILogger
using System;                                   // For ArgumentNullException, Path, Guid, etc.
using System.IO;                                // For Path, File, Directory, FileStream, Stream
using System.Linq;                              // For LINQ methods like Any()
using System.Threading.Tasks;                   // For Task, async/await
using System.Collections.Generic;               // Needed for Aggregate in sanitation

// Adjust namespace to match your project structure
namespace ExcellyGenLMS.Infrastructure.Services.Storage
{
    /// <summary>
    /// Implements IFileStorageService using the local file system.
    /// Stores files relative to a configured base path (e.g., wwwroot/uploads).
    /// </summary>
    public class LocalFileStorageService : IFileStorageService // Implements the correct interface
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<LocalFileStorageService> _logger;
        private readonly string _baseStoragePath; // The absolute physical base path on the server
        private readonly string _baseUrl;         // The base URL of the application for constructing file URLs
        private readonly string _requestPathBase; // The URL path segment corresponding to the _baseStoragePath (e.g., "/uploads")

        public LocalFileStorageService(
            IWebHostEnvironment env,
            IHttpContextAccessor httpContextAccessor,
            ILogger<LocalFileStorageService> logger,
            IConfiguration configuration) // Inject configuration
        {
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // --- Determine Base Physical Storage Path ---
            // (Constructor logic remains the same)
            string configPath = configuration.GetValue<string>("FileStorage:LocalPath") ?? Path.Combine("wwwroot", "uploads");

            if (Path.IsPathRooted(configPath))
            {
                _baseStoragePath = configPath;
                _logger.LogInformation("Using configured absolute local storage path: {Path}", _baseStoragePath);
                _requestPathBase = "/uploads";
            }
            else
            {
                _baseStoragePath = Path.GetFullPath(Path.Combine(_env.ContentRootPath, configPath));
                _logger.LogInformation("Using relative local storage path '{ConfigPath}', resolved to absolute: {Path}", configPath, _baseStoragePath);
                var wwwRootPath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot_fallback"); // Define wwwroot path or fallback
                if (_baseStoragePath.StartsWith(wwwRootPath, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(_env.WebRootPath))
                {
                    _requestPathBase = "/" + Path.GetRelativePath(_env.WebRootPath, _baseStoragePath).Replace(Path.DirectorySeparatorChar, '/');
                }
                else
                {
                    _requestPathBase = "/uploads";
                    _logger.LogWarning("Local storage path is outside wwwroot. Mapped URL path is {RequestPath}.", _requestPathBase);
                }
            }
            _requestPathBase = "/" + _requestPathBase.Trim('/'); // Ensure single leading slash

            try { if (!Directory.Exists(_baseStoragePath)) Directory.CreateDirectory(_baseStoragePath); }
            catch (Exception ex) { _logger.LogError(ex, "FATAL: Failed to create base storage directory: {Path}.", _baseStoragePath); throw new InvalidOperationException($"Failed to initialize storage directory at {_baseStoragePath}", ex); }

            // --- Determine Base URL ---
            var request = _httpContextAccessor.HttpContext?.Request;
            _baseUrl = request != null ? $"{request.Scheme}://{request.Host}" : configuration.GetValue<string>("ApplicationUrl") ?? "http://localhost:5000"; // Use configured App URL or default

            _logger.LogInformation("LocalFileStorageService Initialized. Physical Path: {Path}, Request Path Base: {ReqPath}, Base URL: {Url}", _baseStoragePath, _requestPathBase, _baseUrl);
        }

        // =====================================================================
        // IMPLEMENTATION OF SaveFileAsync MATCHING THE INTERFACE
        // =====================================================================
        public async Task<string?> SaveFileAsync(
            Stream fileStream,          // Expects Stream
            string fileName,            // Expects original filename
            string? contentType,         // Expects content type
            string containerName,
            string[]? allowedExtensions = null,
            double maxFileSizeMB = 5)
        {
            // Check stream validity
            if (fileStream == null || fileStream == Stream.Null || fileStream.Length == 0)
            {
                _logger.LogWarning("SaveFileAsync (Stream): Input stream is null or empty.");
                return null;
            }
            if (string.IsNullOrWhiteSpace(fileName))
            {
                _logger.LogWarning("SaveFileAsync (Stream): File name cannot be empty.");
                return null;
            }

            // --- Validation ---
            _logger.LogDebug("SaveFileAsync (Stream): MaxFileSize hint = {MaxSizeMB}MB for file {FileName}", maxFileSizeMB, fileName);
            // Note: Stream length validation depends on the stream type. Relying on caller's IFormFile.Length check is safer.

            var fileExtension = Path.GetExtension(fileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(fileExtension))
            {
                _logger.LogWarning("SaveFileAsync (Stream): File '{FileName}' has no extension.", fileName);
                return null;
            }

            if (allowedExtensions != null && allowedExtensions.Any() && !allowedExtensions.Contains(fileExtension))
            {
                _logger.LogWarning("SaveFileAsync (Stream): File '{FileName}' has disallowed extension '{Extension}'. Allowed: {AllowedExtensions}",
                                  fileName, fileExtension, string.Join(", ", allowedExtensions));
                return null;
            }

            try
            {
                // --- Path Construction ---
                var sanitizedContainerName = Path.GetInvalidFileNameChars()
                                                 .Aggregate(containerName, (current, c) => current.Replace(c.ToString(), "_"))
                                                 .Replace("..", "_");
                var containerPath = Path.Combine(_baseStoragePath, sanitizedContainerName);

                if (!Directory.Exists(containerPath))
                {
                    Directory.CreateDirectory(containerPath);
                    _logger.LogDebug("Created container directory: {Path}", containerPath);
                }

                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var absoluteFilePath = Path.Combine(containerPath, uniqueFileName);

                _logger.LogInformation("Attempting to save stream for file '{OriginalFileName}' to physical path: {PhysicalPath}", fileName, absoluteFilePath);

                // --- File Saving (from Stream) ---
                if (fileStream.CanSeek) { fileStream.Position = 0; } // Reset position if possible

                using (var destinationStream = new FileStream(absoluteFilePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true)) // Added some FileStream options
                {
                    await fileStream.CopyToAsync(destinationStream);
                    await destinationStream.FlushAsync(); // Ensure data is written
                }

                _logger.LogInformation("File saved successfully from stream: {PhysicalPath}", absoluteFilePath);

                // --- Return Relative Path ---
                var relativePath = Path.Combine(sanitizedContainerName, uniqueFileName).Replace(Path.DirectorySeparatorChar, '/');
                return relativePath;
            }
            catch (IOException ioEx)
            {
                _logger.LogError(ioEx, "IO Error saving stream '{FileName}' to container '{Container}'. Path: {Path}", fileName, containerName, _baseStoragePath);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic Error saving stream '{FileName}' to container '{Container}'. Path: {Path}", fileName, containerName, _baseStoragePath);
                return null;
            }
        }
        // =====================================================================
        // END OF IMPLEMENTATION for SaveFileAsync
        // =====================================================================


        public Task<bool> DeleteFileAsync(string filePath)
        {
            // (Keep existing DeleteFileAsync implementation - it doesn't depend on IFormFile)
            if (string.IsNullOrWhiteSpace(filePath)) { /* ... log ... */ return Task.FromResult(true); }
            try
            {
                var cleanRelativePath = filePath.Replace("..", "").TrimStart('/');
                var physicalPath = Path.Combine(_baseStoragePath, cleanRelativePath.Replace('/', Path.DirectorySeparatorChar));
                if (!Path.GetFullPath(physicalPath).StartsWith(Path.GetFullPath(_baseStoragePath), StringComparison.OrdinalIgnoreCase)) { /* ... log security ... */ return Task.FromResult(false); }
                if (File.Exists(physicalPath)) { File.Delete(physicalPath); _logger.LogInformation("Deleted file: {Path}", physicalPath); }
                else { _logger.LogWarning("Attempted to delete non-existent file: {Path}", physicalPath); }
                return Task.FromResult(true);
            }
            catch (Exception ex) { _logger.LogError(ex, "Error deleting file: {Path}", filePath); return Task.FromResult(false); }
        }

        public string GetFileUrl(string filePath)
        {
            // (Keep existing GetFileUrl implementation)
            if (string.IsNullOrWhiteSpace(filePath)) { /* ... log ... */ return string.Empty; }
            var cleanRelativePath = filePath.Replace(Path.DirectorySeparatorChar, '/').TrimStart('/');
            var fullUrl = $"{_baseUrl.TrimEnd('/')}{_requestPathBase.TrimEnd('/')}/{cleanRelativePath}";
            return fullUrl;
        }
    }
}