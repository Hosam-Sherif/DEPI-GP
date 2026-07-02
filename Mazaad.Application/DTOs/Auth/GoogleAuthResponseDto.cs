// Mazaad.Application/DTOs/Auth/GoogleAuthResponseDto.cs

namespace Mazaad.Application.DTOs.Auth
{
    public class GoogleAuthResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiry { get; set; }
        public UserInfoDto User { get; set; } = null!;

        /// <summary>
        /// لو true → الفرونت لازم يوجّه المستخدم لصفحة استكمال بيانات/مستندات الشركة
        /// (نفس فلو CompanyRegistrationController الموجود عندك).
        /// </summary>
        public bool RequiresCompanyProfileCompletion { get; set; }
    }
}