// Mazaad.Application/DTOs/Auth/AuthResponseDto.cs

namespace Mazaad.Application.DTOs.Auth
{
    // ده اللي بيترجع للـ client بعد login أو register ناجح
    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;

        // بيترجع في HttpOnly Cookie مش في الـ body — أأمن
        public string RefreshToken { get; set; } = string.Empty;

        public DateTime AccessTokenExpiry { get; set; }

        public UserInfoDto User { get; set; } = new();
    }

    public class UserInfoDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();
        public bool TwoFactorEnabled { get; set; }
    }
}