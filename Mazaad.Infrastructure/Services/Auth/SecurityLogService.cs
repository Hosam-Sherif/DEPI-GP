// Mazaad.Infrastructure/Services/Auth/SecurityLogService.cs

using Mazaad.Application.DTOs.Auth;
using Mazaad.Application.Interfaces.Services;
using Mazaad.Domain.Enums;
using Mazaad.Domain.Models;
using Mazaad.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mazaad.Infrastructure.Services.Auth
{
    public class SecurityLogService : ISecurityLogService
    {
        private readonly AppDbContext _context;

        public SecurityLogService(AppDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(
            SecurityEventType eventType,
            bool success,
            string? ipAddress,
            string? userAgent = null,
            int? userId = null,
            string? email = null,
            string? details = null)
        {
            var log = new SecurityLog
            {
                EventType = eventType,
                Success = success,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                UserId = userId,
                Email = email,
                Details = details,
                CreatedAt = DateTime.UtcNow
            };

            _context.SecurityLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<SecurityLogDto>> GetUserLogsAsync(
            int userId,
            int count = 50)
        {
            var logs = await _context.SecurityLogs
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.CreatedAt)
                .Take(count)
                .ToListAsync();

            return logs.Select(MapToDto);
        }

        public async Task<IEnumerable<SecurityLogDto>> GetAllLogsAsync(
            DateTime? from = null,
            DateTime? to = null,
            SecurityEventType? eventType = null,
            int count = 100)
        {
            var query = _context.SecurityLogs.AsQueryable();

            if (from.HasValue)
                query = query.Where(l => l.CreatedAt >= from.Value);

            if (to.HasValue)
                query = query.Where(l => l.CreatedAt <= to.Value);

            if (eventType.HasValue)
                query = query.Where(l => l.EventType == eventType.Value);

            var logs = await query
                .OrderByDescending(l => l.CreatedAt)
                .Take(count)
                .ToListAsync();

            return logs.Select(MapToDto);
        }

        private static SecurityLogDto MapToDto(SecurityLog log) => new()
        {
            Id = log.Id,
            UserId = log.UserId,
            Email = log.Email,
            EventType = log.EventType.ToString(),
            Success = log.Success,
            Details = log.Details,
            IpAddress = log.IpAddress,
            CreatedAt = log.CreatedAt
        };
    }
}