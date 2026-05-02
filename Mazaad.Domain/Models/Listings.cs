using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Mazaad.Domain.Enums;

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

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        /// <summary>Technical specifications as free-text JSON or plain text</summary>
        public string TechnicalSpecs { get; set; } = string.Empty;

        public decimal MinOrderQuantity { get; set; }
        public decimal AvailableQuantity { get; set; }

        /// <summary>Purity / grade percentage (e.g. 99.9)</summary>
        public decimal PurityPercentage { get; set; }

        [MaxLength(10)]
        public string BaseCurrency { get; set; } = "USD";

        /// <summary>Starting / current highest bid price</summary>
        public decimal CurrentHighestBid { get; set; }

        /// <summary>Count of bids placed, incremented on each successful bid</summary>
        public int BidCount { get; set; }

        public ListingStatus Status { get; set; } = ListingStatus.Upcoming;
        public ListingCondition Condition { get; set; } = ListingCondition.New;

        /// <summary>Primary image URL for the marketplace card</summary>
        [MaxLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>Pickup / warehouse location label shown on bidding room map</summary>
        [MaxLength(300)]
        public string Location { get; set; } = string.Empty;

        /// <summary>Due diligence document URLs (comma-separated or JSON array)</summary>
        public string DueDiligenceUrls { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        public Companies Company { get; set; } = null!;
        public Material_Categories Category { get; set; } = null!;
        public ICollection<Bids> Bids { get; set; } = new HashSet<Bids>();
        public ICollection<Chat_Channels> Chat_Channels { get; set; } = new HashSet<Chat_Channels>();
    }
}
