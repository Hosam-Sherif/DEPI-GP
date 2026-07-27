using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Mazaad.Domain.Enums;

namespace Mazaad.Domain.Models
{
    public class Listings
    {
        [Key]
        public int Id { get; set; }

        /// <summary>Null when the listing/auction was created by an individual (non-company) seller.</summary>
        [ForeignKey("Company")]
        public int? CompanyId { get; set; }

        /// <summary>Set only for listings created by an individual (non-company) seller. Null for company listings.</summary>
        [ForeignKey("Seller")]
        public int? SellerUserId { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        /// <summary>Technical specifications as free-text JSON or plain text</summary>
        public string TechnicalSpecs { get; set; } = string.Empty;

        public decimal MinOrderQuantity { get; set; }
        public decimal AvailableQuantity { get; set; }

        /// <summary>Unit of measure for quantity fields (e.g. kg, gm, ton, MT, L, m3)</summary>
        [MaxLength(20)]
        public string UnitOfMeasure { get; set; } = "kg";

        /// <summary>Purity / grade percentage (e.g. 99.9)</summary>
        public decimal PurityPercentage { get; set; }

        [MaxLength(10)]
        public string BaseCurrency { get; set; } = "USD";

        /// <summary>Starting / current highest bid price</summary>
        public decimal CurrentHighestBid { get; set; }

        /// <summary>Count of bids placed, incremented on each successful bid</summary>
        public int BidCount { get; set; }

        public ListingStatus Status { get; set; } = ListingStatus.PendingApproval;
        public ListingCondition Condition { get; set; } = ListingCondition.New;

        /// <summary>SuperAdmin user Id who approved/rejected this listing. Null while pending.</summary>
        public int? ApprovedByUserId { get; set; }

        /// <summary>When the SuperAdmin approved or rejected this listing. Null while pending.</summary>
        public DateTime? ApprovedAt { get; set; }

        /// <summary>Required when the SuperAdmin rejects the listing.</summary>
        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        /// <summary>Primary image URL for the marketplace card</summary>
        [Column("image_url")]
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
        public Companies? Company { get; set; }
        public ApplicationUser? Seller { get; set; }
        public Material_Categories Category { get; set; } = null!;
        public ICollection<Bids> Bids { get; set; } = new HashSet<Bids>();
        public ICollection<Chat_Channels> Chat_Channels { get; set; } = new HashSet<Chat_Channels>();

        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}