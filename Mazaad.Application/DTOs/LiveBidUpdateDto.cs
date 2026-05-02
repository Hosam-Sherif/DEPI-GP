using System;

namespace Mazaad.Application.DTOs
{
    /// <summary>Payload broadcast via SignalR BiddingHub when a new bid is placed.</summary>
    public class LiveBidUpdateDto
    {
        public int ListingId { get; set; }
        public int BidId { get; set; }
        public string DisplayBidderName { get; set; } = string.Empty;
        public decimal NewHighestBid { get; set; }
        public int TotalBidCount { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
