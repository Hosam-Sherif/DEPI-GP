// Mazaad.Application/Interfaces/Services/IAuthService.cs

using Mazaad.Application.Common;
using Mazaad.Application.DTOs.Auth;
using Microsoft.AspNetCore.Http;

namespace Mazaad.Application.Interfaces.Services
{
    public interface IAuthService
    {
        // ── تسجيل user جديد (مش مرتبط بشركة) ──────────
        Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto dto, string ipAddress);

        // ── تسجيل الدخول ────────────────────────────────
        Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto, string ipAddress);

        // ── تسجيل / تسجيل دخول عبر جوجل ─────────────────
        Task<Result<GoogleAuthResponseDto>> GoogleLoginAsync(GoogleLoginDto dto, string ipAddress);

        // ── تجديد الـ Access Token باستخدام Refresh Token
        Task<Result<AuthResponseDto>> RefreshTokenAsync(string refreshToken, string ipAddress);

        // ── تسجيل الخروج + إلغاء الـ Refresh Token ─────
        Task<Result> LogoutAsync(string refreshToken, string ipAddress);

        // ── تغيير كلمة المرور ────────────────────────────
        Task<Result> ChangePasswordAsync(int userId, ChangePasswordDto dto, string ipAddress);

        // ── طلب رابط استعادة كلمة المرور (نسيت كلمة المرور) ──
        Task<Result> ForgotPasswordAsync(ForgotPasswordDto dto, string ipAddress);

        // ── تعيين كلمة مرور جديدة عبر الـ Token ─────────
        Task<Result> ResetPasswordAsync(ResetPasswordDto dto, string ipAddress);

        // ── بيانات الـ profile ───────────────────────────
        Task<Result<MyProfileDto>> GetMyProfileAsync(int userId);

        // ── تعديل بيانات الـ profile ─────────────────────
        Task<Result> UpdateProfileAsync(int userId, UpdateProfileDto dto);

        // ── رفع صورة الـ profile ──────────────────────────
        Task<Result<string>> UploadProfilePictureAsync(int userId, IFormFile file);
    }
}