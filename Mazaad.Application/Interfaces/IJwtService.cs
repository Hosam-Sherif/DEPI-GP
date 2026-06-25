// Mazaad.Application/Interfaces/Services/IJwtService.cs

using Mazaad.Domain.Models;

namespace Mazaad.Application.Interfaces.Services
{
    public interface IJwtService
    {
        // ── يولد Access Token من الـ user و roles بتاعته ─
        Task<string> GenerateAccessTokenAsync(ApplicationUser user, IEnumerable<string> roles);

        // ── يولد Refresh Token آمن (random bytes) ────────
        string GenerateRefreshToken();

        // ── يجيب الـ userId من expired token (للـ refresh flow) ─
        int? GetUserIdFromExpiredToken(string token);
    }
}