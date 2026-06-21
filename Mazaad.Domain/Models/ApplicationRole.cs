using Microsoft.AspNetCore.Identity;

namespace Mazaad.Domain.Models
{
    /// <summary>
    /// Typed role entity. int PK matches ApplicationUser.
    /// System Roles:
    ///   SuperAdmin   — platform-level admin
    ///   CompanyAdmin — manages users inside their own company
    ///   CompanyUser  — standard bidder/operator
    /// </summary>
    public class ApplicationRole : IdentityRole<int>
    {
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}