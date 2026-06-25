namespace Mazaad.Domain.Models
{
    /// <summary>
    /// Server-side refresh token.
    /// Enables secure rotation: each use issues a new pair and revokes the old one.
    /// Stored in DB so any token can be revoked instantly (logout, compromise, etc.).
    /// </summary>
    public class RefreshToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Populated when this token is rotated.
        /// If a revoked token is reused, the whole family is invalidated
        /// (reuse detection / theft detection).
        /// </summary>
        public string? ReplacedByToken { get; set; }

        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsRevoked { get; set; }
        public string? RevokedReason { get; set; }

        public string? CreatedByIp { get; set; }
        public string? RevokedByIp { get; set; }

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsActive => !IsRevoked && !IsExpired;

        // Navigation
        public ApplicationUser User { get; set; } = null!;
    }
}