using System;
using Mazaad.Domain.Enums;

namespace Mazaad.Application.DTOs
{
    public class BidDetailDto
    {
        public int Id { get; set; }
        public int ListingId { get; set; }

        /// <summary>Null when the bid was placed by an individual (non-company) bidder.</summary>
        public int? BuyerCompanyId { get; set; }  

        public string DisplayBidderName { get; set; } = string.Empty;
        public decimal BidAmountPerUnit { get; set; }
        public decimal TotalBidAmount { get; set; }
        public decimal Quantity { get; set; }
        public bool IsAnonymous { get; set; }
        public BidStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}