// Mazaad.Application/DTOs/Auth/UpdateProfileDto.cs

using System.ComponentModel.DataAnnotations;

namespace Mazaad.Application.DTOs.Auth
{
    // اللي الـ user يقدر يعدله بنفسه (مش بيشمل الإيميل أو الـ Company - دول محتاجين تحقق/صلاحيات منفصلة)
    public class UpdateProfileDto
    {
        [Required, MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? JobTitle { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }
    }
}