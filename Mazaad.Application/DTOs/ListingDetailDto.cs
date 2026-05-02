using System;
using System.Collections.Generic;
using Mazaad.Domain.Enums;

namespace Mazaad.Application.DTOs
{
    /// <summary>Full listing detail for the Live Bidding Room screen.</summary>
    public class ListingDetailDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TechnicalSpecs { get; set; } = string.Empty;
        public decimal MinOrderQuantity { get; set; }
        public decimal AvailableQuantity { get; set; }
        public decimal PurityPercentage { get; set; }
        public string BaseCurrency { get; set; } = "USD";
        public decimal CurrentHighestBid { get; set; }
        public int BidCount { get; set; }
        public ListingStatus Status { get; set; }
        public ListingCondition Condition { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string DueDiligenceUrls { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        /// <summary>Top 5 bids ordered descending by amount, shown in Live Bidding Feed</summary>
        public IEnumerable<BidDetailDto> TopBids { get; set; } = new List<BidDetailDto>();
    }
}
