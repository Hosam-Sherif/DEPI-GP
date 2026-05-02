using System;
using Mazaad.Domain.Enums;

namespace Mazaad.Application.DTOs
{
    /// <summary>Response DTO for marketplace listing cards with all preview fields.</summary>
    public class ListingCardDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public decimal CurrentHighestBid { get; set; }
        public int BidCount { get; set; }
        public ListingStatus Status { get; set; }
        public ListingCondition Condition { get; set; }
        public string BaseCurrency { get; set; } = "USD";
        public DateTime EndDate { get; set; }

        /// <summary>Seconds remaining until auction ends (negative if ended)</summary>
        public double SecondsRemaining => (EndDate - DateTime.UtcNow).TotalSeconds;
    }
}
