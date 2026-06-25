// Mazaad.Application/DTOs/Company/CompanyUserDto.cs

using System.ComponentModel.DataAnnotations;

namespace Mazaad.Application.DTOs.Company
{
    // بيتبعت من CompanyAdmin لإضافة user جديد لشركته
    public class AddCompanyUserDto
    {
        [Required, MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [MaxLength(100)]
        public string JobTitle { get; set; } = string.Empty;

        // CompanyAdmin أو CompanyUser
        [Required]
        public string Role { get; set; } = string.Empty;
    }

    // بيترجع في قائمة users الشركة
    public class CompanyUserResponseDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();
        public bool IsActive { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // بيتبعت لتغيير role أو تفعيل/تعطيل user
    public class UpdateCompanyUserDto
    {
        public string? Role { get; set; }
        public bool? IsActive { get; set; }
    }
}