using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Mazaad.Domain.Enums;

namespace Mazaad.Domain.Models
{
    public class Bids
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Listing")]
        public int ListingId { get; set; }

        [ForeignKey("User")]
        public int PlacedByUserId { get; set; }

        /// <summary>Null when the bid was placed by an individual (non-company) bidder.</summary>  //  تعديل (تعليق جديد)
        [ForeignKey("BuyerCompany")]
        public int? BuyerCompanyId { get; set; }   //  تعديل: كانت int بقت int? (اختياري)

        public decimal BidAmountPerUnit { get; set; }
        public decimal TotalBidAmount { get; set; }
        public decimal Quantity { get; set; }
        public bool IsAnonymous { get; set; }

        public bool WinningBid { get; set; }
        public BidStatus Status { get; set; } = BidStatus.Active;
        public DateTime CreatedAt { get; set; }

        // Navigation
        public Listings Listing { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
        public Companies? BuyerCompany { get; set; }   //  تعديل: كانت Companies BuyerCompany = null! بقت Companies? (nullable)
        public ICollection<Orders> Orders { get; set; } = new HashSet<Orders>();
    }
}