// Mazaad.Application/DTOs/Auth/MyProfileDto.cs

namespace Mazaad.Application.DTOs.Auth
{
    // بيترجع لما الـ user يفتح صفحة My Account
    public class MyProfileDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? JobTitle { get; set; }
        public string? PhoneNumber { get; set; }
        public int? CompanyId { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }
}