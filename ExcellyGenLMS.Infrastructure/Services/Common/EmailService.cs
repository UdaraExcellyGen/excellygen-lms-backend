using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ExcellyGenLMS.Application.Interfaces.Common;
using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace ExcellyGenLMS.Infrastructure.Services.Common
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly bool _useSsl;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly string _appName;
        private readonly string _appUrl;

        public EmailService(
            IConfiguration configuration,
            ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // Get SMTP settings from configuration
            _smtpHost = configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(configuration["Email:SmtpPort"] ?? "587");
            _useSsl = bool.Parse(configuration["Email:UseSsl"] ?? "true");
            _smtpUsername = configuration["Email:Username"] ?? "";
            _smtpPassword = configuration["Email:Password"] ?? "";
            _fromEmail = configuration["Email:FromEmail"] ?? "noreply@excellygenlms.com";
            _fromName = configuration["Email:FromName"] ?? "ExcellyGenLMS";
            _appName = configuration["AppSettings:Name"] ?? "ExcellyGenLMS";
            _appUrl = configuration["AppSettings:Url"] ?? "https://excelly-lms-f3500.web.app";
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                _logger.LogInformation($"Sending email to {to} with subject: {subject}");

                if (string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
                {
                    _logger.LogWarning("SMTP credentials not configured. Email not sent.");
                    return false;
                }

                using (var client = new SmtpClient(_smtpHost, _smtpPort))
                {
                    client.EnableSsl = _useSsl;
                    client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;

                    using (var message = new MailMessage())
                    {
                        message.From = new MailAddress(_fromEmail, _fromName);
                        message.Subject = subject;
                        message.Body = body;
                        message.IsBodyHtml = isHtml;
                        message.To.Add(new MailAddress(to));

                        await client.SendMailAsync(message);
                    }
                }

                _logger.LogInformation($"Email sent successfully to {to}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending email to {to}");
                return false;
            }
        }

        public async Task<bool> SendWelcomeEmailAsync(string to, string userName)
        {
            var subject = $"Welcome to {_appName}";
            var body = GenerateWelcomeEmailBody(userName);
            return await SendEmailAsync(to, subject, body);
        }

        public async Task<bool> SendTemporaryPasswordEmailAsync(string to, string userName, string tempPassword)
        {
            var subject = $"Your Temporary Password for {_appName}";
            var body = GenerateTemporaryPasswordEmailBody(userName, tempPassword);
            return await SendEmailAsync(to, subject, body);
        }

        public async Task<bool> SendPasswordResetLinkAsync(string to, string resetLink)
        {
            var subject = $"Reset Your {_appName} Password";
            var body = GeneratePasswordResetEmailBody(resetLink);
            return await SendEmailAsync(to, subject, body);
        }

        #region Email Templates

        private string GenerateWelcomeEmailBody(string userName)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }");
            sb.AppendLine(".container { max-width: 600px; margin: 0 auto; padding: 20px; }");
            sb.AppendLine(".header { background-color: #4a6cf7; color: white; padding: 20px; text-align: center; }");
            sb.AppendLine(".content { padding: 20px; }");
            sb.AppendLine(".footer { text-align: center; margin-top: 20px; font-size: 12px; color: #666; }");
            sb.AppendLine(".button { display: inline-block; background-color: #4a6cf7; color: white; padding: 10px 20px; text-decoration: none; border-radius: 4px; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class='container'>");
            sb.AppendLine("<div class='header'>");
            sb.AppendLine($"<h1>Welcome to {_appName}</h1>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class='content'>");
            sb.AppendLine($"<p>Hello {userName},</p>");
            sb.AppendLine($"<p>Welcome to {_appName}! Your account has been successfully created.</p>");
            sb.AppendLine("<p>You can now log in to access all features and resources available to you.</p>");
            sb.AppendLine($"<p><a href='{_appUrl}' class='button'>Log In Now</a></p>");
            sb.AppendLine("<p>If you have any questions or need assistance, please contact your administrator.</p>");
            sb.AppendLine("<p>Thank you,<br>The ExcellyGenLMS Team</p>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class='footer'>");
            sb.AppendLine($"<p>&copy; {DateTime.Now.Year} {_appName}. All rights reserved.</p>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        private string GenerateTemporaryPasswordEmailBody(string userName, string tempPassword)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }");
            sb.AppendLine(".container { max-width: 600px; margin: 0 auto; padding: 20px; }");
            sb.AppendLine(".header { background-color: #4a6cf7; color: white; padding: 20px; text-align: center; }");
            sb.AppendLine(".content { padding: 20px; }");
            sb.AppendLine(".footer { text-align: center; margin-top: 20px; font-size: 12px; color: #666; }");
            sb.AppendLine(".password-box { background-color: #f5f5f5; border: 1px solid #ddd; padding: 15px; margin: 20px 0; text-align: center; font-size: 18px; letter-spacing: 1px; }");
            sb.AppendLine(".alert { background-color: #fff8e1; border-left: 4px solid #ffc107; padding: 10px; margin: 15px 0; }");
            sb.AppendLine(".button { display: inline-block; background-color: #4a6cf7; color: white; padding: 10px 20px; text-decoration: none; border-radius: 4px; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class='container'>");
            sb.AppendLine("<div class='header'>");
            sb.AppendLine($"<h1>Your Temporary Password</h1>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class='content'>");
            sb.AppendLine($"<p>Hello {userName},</p>");
            sb.AppendLine($"<p>A temporary password has been generated for your {_appName} account.</p>");
            sb.AppendLine("<div class='password-box'>");
            sb.AppendLine($"{tempPassword}");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class='alert'>");
            sb.AppendLine("<p><strong>Important:</strong> You will be required to change this password when you first log in.</p>");
            sb.AppendLine("</div>");
            sb.AppendLine($"<p><a href='{_appUrl}' class='button'>Log In Now</a></p>");
            sb.AppendLine("<p>If you did not request this password, please contact your administrator immediately.</p>");
            sb.AppendLine("<p>Thank you,<br>The ExcellyGenLMS Team</p>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class='footer'>");
            sb.AppendLine($"<p>&copy; {DateTime.Now.Year} {_appName}. All rights reserved.</p>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        private string GeneratePasswordResetEmailBody(string resetLink)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }");
            sb.AppendLine(".container { max-width: 600px; margin: 0 auto; padding: 20px; }");
            sb.AppendLine(".header { background-color: #4a6cf7; color: white; padding: 20px; text-align: center; }");
            sb.AppendLine(".content { padding: 20px; }");
            sb.AppendLine(".footer { text-align: center; margin-top: 20px; font-size: 12px; color: #666; }");
            sb.AppendLine(".alert { background-color: #fff8e1; border-left: 4px solid #ffc107; padding: 10px; margin: 15px 0; }");
            sb.AppendLine(".button { display: inline-block; background-color: #4a6cf7; color: white; padding: 10px 20px; text-decoration: none; border-radius: 4px; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class='container'>");
            sb.AppendLine("<div class='header'>");
            sb.AppendLine($"<h1>Reset Your Password</h1>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class='content'>");
            sb.AppendLine($"<p>Hello,</p>");
            sb.AppendLine($"<p>We received a request to reset your password for your {_appName} account.</p>");
            sb.AppendLine("<p>To reset your password, please click the button below:</p>");
            sb.AppendLine($"<p><a href='{resetLink}' class='button'>Reset Password</a></p>");
            sb.AppendLine("<div class='alert'>");
            sb.AppendLine("<p><strong>Note:</strong> This link is valid for 24 hours. If you don't use it within that time, you'll need to request another password reset.</p>");
            sb.AppendLine("</div>");
            sb.AppendLine("<p>If you did not request a password reset, please ignore this email or contact your administrator.</p>");
            sb.AppendLine("<p>Thank you,<br>The ExcellyGenLMS Team</p>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class='footer'>");
            sb.AppendLine($"<p>&copy; {DateTime.Now.Year} {_appName}. All rights reserved.</p>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        #endregion
    }
}