namespace Mazaad.Application.DTOs.Analytics
{
    /// <summary>Average bid price per material category for active listings.</summary>
    public class AssetValueIndexDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal AverageBidPrice { get; set; }
        public decimal HighestBid { get; set; }
        public decimal LowestBid { get; set; }
        public int ActiveListingsCount { get; set; }
        public string BaseCurrency { get; set; } = "USD";
    }
}