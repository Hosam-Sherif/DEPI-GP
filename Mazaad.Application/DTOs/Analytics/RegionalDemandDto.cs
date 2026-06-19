namespace Mazaad.Application.DTOs.Analytics
{
    /// <summary>Bid and order activity grouped by city/region.</summary>
    public class RegionalDemandDto
    {
        public string Region { get; set; } = string.Empty;
        public int TotalBids { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalOrderValue { get; set; }
        public int ActiveListings { get; set; }
        /// <summary>Demand intensity score 0–100 (normalized across all regions).</summary>
        public int DemandScore { get; set; }
    }
}