using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public bool WinningBid { get; set; }
        public DateTime CreatedAt { get; set; }

        public Listings Listing { get; set; }
        public App_Users User { get; set; }
        public Companies BuyerCompany { get; set; }

        public ICollection<Orders> Orders { get; set; } = new HashSet<Orders>();
    }
}
