using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mazaad.Domain.Models
{
    public class Listings
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Company")]
        public int CompanyId { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public decimal MinOrderQuantity { get; set; }
        public decimal AvailableQuantity { get; set; }
        public decimal PurityPercentage { get; set; }
        public string BaseCurrency { get; set; }
        public decimal CurrentHighestBid { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Companies Company { get; set; }
        public Material_Categories Category { get; set; }
        public ICollection<Bids> Bids { get; set; } = new HashSet<Bids>();
        public ICollection<Chat_Channels> Chat_Channels { get; set; } = new HashSet<Chat_Channels>();
    }
}
