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
        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public string ReferenceType { get; set; }
        public int ReferenceId { get; set; }
        public DateTime CreatedAt { get; set; }

        public App_Users User { get; set; }
    }
}
