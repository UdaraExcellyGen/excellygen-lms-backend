// ExcellyGenLMS.API/Controllers/Common/ImageProxyController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ExcellyGenLMS.API.Controllers.Common
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Ensure only logged-in users can use the proxy
    public class ImageProxyController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ImageProxyController> _logger;

        public ImageProxyController(IHttpClientFactory httpClientFactory, ILogger<ImageProxyController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpGet("proxy")]
        public async Task<IActionResult> GetImageProxy([FromQuery] string url)
        {
            if (string.IsNullOrEmpty(url) || !Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return BadRequest("A valid URL must be provided.");
            }

            // Optional: Add a whitelist check to ensure you only proxy images from your Firebase storage
            if (!uri.Host.EndsWith("firebasestorage.googleapis.com"))
            {
                _logger.LogWarning("Image proxy request for a non-whitelisted domain was blocked: {Domain}", uri.Host);
                return Forbid("Proxying from this domain is not allowed.");
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync(uri);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to fetch image from URL {Url}. Status: {StatusCode}", url, response.StatusCode);
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                var imageBytes = await response.Content.ReadAsByteArrayAsync();
                var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";

                return File(imageBytes, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error proxying image from URL {Url}", url);
                return StatusCode(500, "An internal server error occurred while proxying the image.");
            }
        }
    }
}