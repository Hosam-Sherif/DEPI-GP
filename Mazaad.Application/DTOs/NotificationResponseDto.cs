using System;

namespace Mazaad.Application.DTOs
{
    /// <summary>User notification record.</summary>
    public class NotificationResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public string ReferenceType { get; set; } = string.Empty;
        public int ReferenceId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
