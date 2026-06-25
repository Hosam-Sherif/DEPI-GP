using Microsoft.AspNetCore.Identity;

namespace Mazaad.Domain.Models
{
    /// <summary>
    /// Primary user entity. Inherits IdentityUser so ASP.NET Identity
    /// manages password hashing, lockout, 2FA tokens, claims, etc.
    /// Replaces the old App_Users model entirely.
    /// int PK stays consistent with the existing schema.
    /// </summary>
    public class ApplicationUser : IdentityUser<int>
    {
        public string FullName { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;

        /// <summary>Null only for SuperAdmin users not tied to a company.</summary>
        public int? CompanyId { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Companies? Company { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new HashSet<RefreshToken>();
        public ICollection<SecurityLog> SecurityLogs { get; set; } = new HashSet<SecurityLog>();

        public ICollection<Messages> Messages { get; set; } = new HashSet<Messages>();
        public ICollection<Bids> Bids { get; set; } = new HashSet<Bids>();
        public ICollection<Notifications> Notifications { get; set; } = new HashSet<Notifications>();
    }
}