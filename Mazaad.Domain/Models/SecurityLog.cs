using Mazaad.Domain.Enums;

namespace Mazaad.Domain.Models
{
    /// <summary>
    /// Immutable audit record for every security-relevant event.
    /// Append-only — never updated or deleted.
    /// Covers: login, logout, 2FA, password events, role changes,
    ///         account lockout, token refresh/revoke, company events.
    /// </summary>
    public class SecurityLog
    {
        public int Id { get; set; }

        /// <summary>Null for anonymous events (e.g. failed login with unknown email).</summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Captured independently from UserId so failed logins
        /// with unknown emails are still logged with the attempted email.
        /// </summary>
        public string? Email { get; set; }

        public SecurityEventType EventType { get; set; }

        public bool Success { get; set; }

        /// <summary>Human-readable detail: "Invalid password — attempt 3/5".</summary>
        public string? Details { get; set; }

        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }

        /// <summary>UTC timestamp — never null, set at creation.</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ApplicationUser? User { get; set; }
    }
}