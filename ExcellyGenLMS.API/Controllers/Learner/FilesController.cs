// ExcellyGenLMS.API/Controllers/FilesController.cs
using ExcellyGenLMS.Application.Interfaces.Common; // Your IFileService namespace
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO; // For Path
using System.Linq; // For Contains
using System.Threading.Tasks;

namespace ExcellyGenLMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly ILogger<FilesController> _logger;

        public FilesController(IFileService fileService, ILogger<FilesController> logger)
        {
            _fileService = fileService;
            _logger = logger;
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
            if (file.Length > 5 * 1024 * 1024) // 5MB limit example
            {
                return BadRequest(new { message = "File size exceeds the 5MB limit." });
            }

            try
            {
                // Your FileService.SaveFileAsync returns a relative path
                var relativePath = await _fileService.SaveFileAsync(file, "forum"); // "forum" is the subfolder

                // Your FileService.GetFullImageUrl can construct the full URL
                var fullImageUrl = _fileService.GetFullImageUrl(relativePath);

                _logger.LogInformation("Forum image uploaded successfully. RelativePath: {RelativePath}, FullUrl: {FullUrl}", relativePath, fullImageUrl);

                // Decide what to return: full URL or relative path?
                // Frontend usually prefers the full URL for immediate display.
                // If backend services/DTOs store relative paths, the GetFullImageUrl is used when populating DTOs.
                return Ok(new { imageUrl = fullImageUrl, relativePath = relativePath }); // Returning both can be useful
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

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteFile([FromQuery] string relativePath) // Expecting relative path e.g., /uploads/forum/image.jpg
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return BadRequest(new { message = "File relative path is required." });
            }

            // Basic validation for relative path format
            if (!relativePath.StartsWith("/uploads/"))
            {
                return BadRequest(new { message = "Invalid relative path format for deletion." });
            }

            try
            {
                // IMPORTANT: Add proper authorization here in a real application.
                // Ensure the current user is allowed to delete this specific file.
                // This might involve checking ownership or specific roles.
                // E.g., if relativePath is stored with an entity, check user owns that entity.

                await _fileService.DeleteFileAsync(relativePath);
                _logger.LogInformation("File deleted successfully based on relative path: {RelativePath}", relativePath);
                return Ok(new { message = "File deleted successfully." });
            }
            catch (FileNotFoundException ex) // Can be more specific if DeleteFileAsync throws it
            {
                _logger.LogWarning("File not found for deletion: {RelativePath} - {Message}", relativePath, ex.Message);
                return NotFound(new { message = "File not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file (relative path: {RelativePath})", relativePath);
                return StatusCode(500, new { message = "An error occurred while deleting the file." });
            }
        }
    }
}