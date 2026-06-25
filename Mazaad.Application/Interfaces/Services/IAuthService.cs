// Mazaad.Application/Interfaces/Services/IAuthService.cs

using Mazaad.Application.Common;
using Mazaad.Application.DTOs.Auth;

namespace Mazaad.Application.Interfaces.Services
{
    public interface IAuthService
    {
        // ── تسجيل user جديد (مش مرتبط بشركة) ──────────
        Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto dto, string ipAddress);

        // ── تسجيل الدخول ────────────────────────────────
        Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto, string ipAddress);

        // ── تجديد الـ Access Token باستخدام Refresh Token
        Task<Result<AuthResponseDto>> RefreshTokenAsync(string refreshToken, string ipAddress);

        // ── تسجيل الخروج + إلغاء الـ Refresh Token ─────
        Task<Result> LogoutAsync(string refreshToken, string ipAddress);

        // ── تغيير كلمة المرور ────────────────────────────
        Task<Result> ChangePasswordAsync(int userId, ChangePasswordDto dto, string ipAddress);
    }
}