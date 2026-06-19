namespace Mazaad.Application.DTOs.Analytics
{
    /// <summary>Listings with highest bidding activity in the last 7 days.</summary>
    public class MomentumMoverDto
    {
        public int ListingId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public decimal CurrentHighestBid { get; set; }
        public string BaseCurrency { get; set; } = "USD";
        public int BidsLast7Days { get; set; }
        public int TotalBids { get; set; }
        /// <summary>Percentage increase in bid activity vs. previous 7-day window.</summary>
        public decimal MomentumGrowthPercent { get; set; }
        public DateTime EndDate { get; set; }
    }
}