// Mazaad.Application/Interfaces/Services/IContactService.cs

using Mazaad.Application.Common;
using Mazaad.Application.DTOs.Contact;

namespace Mazaad.Application.Interfaces.Services
{
    public interface IContactService
    {
        /// <summary>
        /// يستقبل رسالة "تواصل معنا" ويبعتها على إيميل الدعم.
        /// </summary>
        Task<Result> SendContactMessageAsync(ContactDto dto);
    }
}