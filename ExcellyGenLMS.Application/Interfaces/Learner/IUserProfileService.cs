using System.Threading.Tasks;
using ExcellyGenLMS.Application.DTOs.Learner;
using Microsoft.AspNetCore.Http;

namespace ExcellyGenLMS.Application.Interfaces.Learner
{
    public interface IUserProfileService
    {
        Task<UserProfileDto> GetUserProfileAsync(string userId);
        Task<UserProfileDto> UpdateUserProfileAsync(string userId, UpdateUserProfileDto updateDto);
        Task<string> UploadUserAvatarAsync(string userId, IFormFile avatar);
        Task DeleteUserAvatarAsync(string userId);
    }
}