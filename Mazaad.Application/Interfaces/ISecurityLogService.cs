// Mazaad.Application/Interfaces/Services/ISecurityLogService.cs

using Mazaad.Application.DTOs.Auth;
using Mazaad.Domain.Enums;

namespace Mazaad.Application.Interfaces.Services
{
    public interface ISecurityLogService
    {
        // ── يسجل event جديد (يتكال internally من باقي الـ services) ──
        Task LogAsync(
            SecurityEventType eventType,
            bool success,
            string? ipAddress,
            string? userAgent = null,
            int? userId = null,
            string? email = null,
            string? details = null);

        // ── يجيب سجلات user معين (للـ user نفسه أو للـ admin) ──────
        Task<IEnumerable<SecurityLogDto>> GetUserLogsAsync(int userId, int count = 50);

        // ── SuperAdmin: يجيب كل السجلات مع فلترة ─────────────────
        Task<IEnumerable<SecurityLogDto>> GetAllLogsAsync(
            DateTime? from = null,
            DateTime? to = null,
            SecurityEventType? eventType = null,
            int count = 100);
    }
}