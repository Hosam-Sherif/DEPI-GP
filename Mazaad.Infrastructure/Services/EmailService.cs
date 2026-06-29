// Mazaad.Infrastructure/Services/EmailService.cs

using Mazaad.Application.Common;
using Mazaad.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace Mazaad.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<Result> SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = subject;

                message.Body = new BodyBuilder
                {
                    HtmlBody = htmlBody
                }.ToMessageBody();

                using var client = new SmtpClient();

                await client.ConnectAsync(_settings.SmtpHost, 465, SecureSocketOptions.SslOnConnect);
                await client.AuthenticateAsync(_settings.SenderEmail, _settings.SenderPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "فشل إرسال الإيميل إلى {ToEmail}", toEmail);
                return Result.Failure(ex.Message);
            }
        }
    }
}