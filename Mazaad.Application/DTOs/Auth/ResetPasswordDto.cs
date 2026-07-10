// Mazaad.Application/DTOs/Auth/ResetPasswordDto.cs

using System.ComponentModel.DataAnnotations;

namespace Mazaad.Application.DTOs.Auth
{
    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب.")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// الـ Token اللي جه من رابط الإيميل (مش مهم يكون encoded أو decoded —
        /// الـ AuthService بيعمل Uri.UnescapeDataString قبل ما يستخدمه).
        /// </summary>
        [Required(ErrorMessage = "رمز إعادة التعيين مطلوب.")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة.")]
        [MinLength(8, ErrorMessage = "كلمة المرور لا تقل عن 8 أحرف.")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$",
            ErrorMessage = "كلمة المرور يجب أن تحتوي على حرف كبير وصغير ورقم ورمز خاص.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب.")]
        [Compare(nameof(NewPassword), ErrorMessage = "كلمتا المرور غير متطابقتين.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}