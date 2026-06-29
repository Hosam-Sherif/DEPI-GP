// Mazaad.Application/Interfaces/Services/IEmailService.cs

using Mazaad.Application.Common;

namespace Mazaad.Application.Interfaces.Services
{
    public interface IEmailService
    {
        /// <summary>
        /// يبعت إيميل بسيط (HTML body) من السيستم لأي مستلم.
        /// </summary>
        Task<Result> SendEmailAsync(string toEmail, string subject, string htmlBody);
    }
}