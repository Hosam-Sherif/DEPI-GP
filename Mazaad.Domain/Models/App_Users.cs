using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mazaad.Domain.Models
{
    public class App_Users
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Company")]
        public int CompanyId { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime LastLoginDate { get; set; }
        public DateTime CreatedAt { get; set; }

        public Companies Company { get; set; } = null!;
        public ICollection<Messages> Messages { get; set; } = new HashSet<Messages>();
        public ICollection<Bids> Bids { get; set; } = new HashSet<Bids>();
        public ICollection<Notifications> Notifications { get; set; } = new HashSet<Notifications>();
    }
}
