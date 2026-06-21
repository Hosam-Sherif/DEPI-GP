// Mazaad.Application/Interfaces/Services/ITwoFactorService.cs

using Mazaad.Application.Common;
using Mazaad.Application.DTOs.Auth;

namespace Mazaad.Application.Interfaces.Services
{
    public interface ITwoFactorService
    {
        // ── يجيب الـ QR Code لتفعيل الـ 2FA ─────────────
        Task<Result<TwoFactorSetupDto>> GetSetupInfoAsync(int userId);

        // ── يفعّل الـ 2FA بعد التحقق من الـ code ──────────
        Task<Result> EnableAsync(int userId, TwoFactorToggleDto dto, string ipAddress);

        // ── يلغي الـ 2FA ──────────────────────────────────
        Task<Result> DisableAsync(int userId, TwoFactorToggleDto dto, string ipAddress);

        // ── يتحقق من الـ code في الـ 2FA login step ───────
        Task<Result<AuthResponseDto>> VerifyAndLoginAsync(TwoFactorVerifyDto dto, string ipAddress);
    }
}