using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Mazaad.Domain.Models
{
    public class Notifications
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public string ReferenceType { get; set; } = string.Empty;
        public int ReferenceId { get; set; }
        public DateTime CreatedAt { get; set; }

        public ApplicationUser User { get; set; } = null!;
    }
}
