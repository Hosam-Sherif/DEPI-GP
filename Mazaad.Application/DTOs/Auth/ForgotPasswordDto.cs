// Mazaad.Application/DTOs/Auth/ForgotPasswordDto.cs

using System.ComponentModel.DataAnnotations;

namespace Mazaad.Application.DTOs.Auth
{
    public class ForgotPasswordDto
    {
        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordDto
    {
        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// الـ Token اللي اتولّد من GeneratePasswordResetTokenAsync وجه في لينك الإيميل.
        /// بييجي decoded تلقائيًا من query params في الفرونت قبل ما يتبعت هنا.
        /// </summary>
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required, MinLength(8)]
        public string NewPassword { get; set; } = string.Empty;

        [Required, Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}