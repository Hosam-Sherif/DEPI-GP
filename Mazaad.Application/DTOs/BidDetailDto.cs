using System;
using Mazaad.Domain.Enums;

namespace Mazaad.Application.DTOs
{
    /// <summary>Individual bid detail record shown in the Live Bidding Feed.</summary>
    public class BidDetailDto
    {
        public int Id { get; set; }
        public int ListingId { get; set; }
        public int BuyerCompanyId { get; set; }

        /// <summary>Displayed bidder name: 'Anonymous' or company name</summary>
        public string DisplayBidderName { get; set; } = string.Empty;
        public decimal BidAmountPerUnit { get; set; }
        public decimal TotalBidAmount { get; set; }
        public decimal Quantity { get; set; }
        public bool IsAnonymous { get; set; }
        public BidStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
