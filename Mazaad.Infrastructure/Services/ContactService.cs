// Mazaad.Infrastructure/Services/ContactService.cs

using Mazaad.Application.Common;
using Mazaad.Application.DTOs.Contact;
using Mazaad.Application.Interfaces.Services;
using Microsoft.Extensions.Options;

namespace Mazaad.Infrastructure.Services
{
    public class ContactService : IContactService
    {
        private readonly IEmailService _emailService;
        private readonly EmailSettings _settings;

        public ContactService(IEmailService emailService, IOptions<EmailSettings> settings)
        {
            _emailService = emailService;
            _settings = settings.Value;
        }

        public async Task<Result> SendContactMessageAsync(ContactDto dto)
        {
            var subject = $"رسالة تواصل جديدة من {dto.Name}";

            var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; direction: rtl; text-align: right;'>
                    <h2>رسالة جديدة من نموذج تواصل معنا</h2>
                    <p><strong>الاسم:</strong> {dto.Name}</p>
                    <p><strong>الإيميل:</strong> {dto.Email}</p>
                    <p><strong>الرسالة:</strong></p>
                    <p>{dto.Message}</p>
                </div>";

            // بنبعت الرسالة لإيميل الدعم المحدد في الإعدادات
            var result = await _emailService.SendEmailAsync(_settings.ToEmail, subject, htmlBody);

            return result;
        }
    }
}