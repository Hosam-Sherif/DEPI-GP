// Mazaad.Application/DTOs/Auth/SecurityLogDto.cs

using Mazaad.Domain.Enums;

namespace Mazaad.Application.DTOs.Auth
{
    // بيترجع في الـ security logs endpoint
    public class SecurityLogDto
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string? Email { get; set; }
        public string EventType { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? Details { get; set; }
        public string? IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}