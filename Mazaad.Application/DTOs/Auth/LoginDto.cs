// Mazaad.Application/DTOs/Auth/LoginDto.cs

using System.ComponentModel.DataAnnotations;

namespace Mazaad.Application.DTOs.Auth
{
    public class LoginDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// لو true، الـ Refresh Token بيتحفظ لـ 30 يوم بدل 7 أيام
        /// </summary>
        public bool RememberMe { get; set; }
    }
}