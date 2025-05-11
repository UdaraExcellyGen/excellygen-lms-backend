using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Interfaces.Common
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        Task<bool> SendWelcomeEmailAsync(string to, string userName);
        Task<bool> SendTemporaryPasswordEmailAsync(string to, string userName, string tempPassword);
        Task<bool> SendPasswordResetLinkAsync(string to, string resetLink);
    }
}