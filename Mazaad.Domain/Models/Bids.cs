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

        [ForeignKey("BuyerCompany")]
        public int BuyerCompanyId { get; set; }

        public decimal BidAmountPerUnit { get; set; }
        public decimal TotalBidAmount { get; set; }
        public decimal Quantity { get; set; }
        public bool IsAnonymous { get; set; }

        /// <summary>Marks the auction-winner bid; set during finalization</summary>
        public bool WinningBid { get; set; }

        /// <summary>Lifecycle status of this individual bid</summary>
        public BidStatus Status { get; set; } = BidStatus.Active;

        public DateTime CreatedAt { get; set; }

        // Navigation
        public Listings Listing { get; set; } = null!;
        public App_Users User { get; set; } = null!;
        public Companies BuyerCompany { get; set; } = null!;
        public ICollection<Orders> Orders { get; set; } = new HashSet<Orders>();
    }
}
