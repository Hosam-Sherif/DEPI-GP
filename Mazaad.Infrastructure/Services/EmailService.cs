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
                _logger.LogInformation(
                    "جاري إرسال إيميل إلى {ToEmail} | Subject: {Subject} | SmtpHost: {Host}:{Port}",
                    toEmail, subject, _settings.SmtpHost, _settings.SmtpPort);

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = subject;

                message.Body = new BodyBuilder
                {
                    HtmlBody = htmlBody
                }.ToMessageBody();

                using var client = new SmtpClient();

                await client.ConnectAsync(
                    _settings.SmtpHost,
                    _settings.SmtpPort,
                    SecureSocketOptions.SslOnConnect);

                await client.AuthenticateAsync("resend", _settings.SenderPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("✅ تم إرسال الإيميل بنجاح إلى {ToEmail}", toEmail);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ فشل إرسال الإيميل إلى {ToEmail} | Error: {Message}",
                    toEmail, ex.Message);

                return Result.Failure(ex.Message);
            }
        }
    }
}