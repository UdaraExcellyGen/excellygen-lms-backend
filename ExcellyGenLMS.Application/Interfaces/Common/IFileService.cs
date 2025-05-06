using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Interfaces.Common
{
    public interface IFileService
    {
        string GetBaseUrl();
        string GetFullImageUrl(string? relativePath);
        Task<string> SaveFileAsync(IFormFile file, string subFolder);
        Task DeleteFileAsync(string? relativePath);
    }
}