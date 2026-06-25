// Mazaad.Application/DTOs/Auth/RegisterDto.cs

using System.ComponentModel.DataAnnotations;

namespace Mazaad.Application.DTOs.Auth
{
    public class RegisterDto
    {
        [Required, MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [Required, Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;

        [MaxLength(100)]
        public string JobTitle { get; set; } = string.Empty;

        // إذا كان بيسجل كـ CompanyAdmin لشركة جديدة
        public int? CompanyId { get; set; }
    }
}