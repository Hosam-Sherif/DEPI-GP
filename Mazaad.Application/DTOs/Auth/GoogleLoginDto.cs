// Mazaad.Application/DTOs/Auth/GoogleLoginDto.cs

namespace Mazaad.Application.DTOs.Auth
{
    public class GoogleLoginDto
    {
        /// <summary>
        /// الـ ID Token اللي راجع من Google Identity Services في الفرونت.
        /// </summary>
        public string IdToken { get; set; } = string.Empty;

        /// <summary>
        /// "Bidder" أو "Company" — يحدد نوع الحساب في حالة كان المستخدم جديد فقط.
        /// لو فاضي أو null بنعتبره Bidder افتراضيًا.
        /// </summary>
        public string? AccountType { get; set; }
    }
}