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

        // ⚠️ CompanyId اتشال نهائيًا.
        // الانضمام لشركة بيتم فقط عن طريق CompanyAdmin
        // من CompanyUsersController.AddUser — مش عن طريق
        // الـ user نفسه وقت التسجيل، عشان منمنعش أي حد
        // يحط companyId جاهز في الـ request ويرتبط بشركة من غير إذن.
    }
}