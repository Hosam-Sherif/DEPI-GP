namespace Mazaad.Application.DTOs.Analytics
{
    /// <summary>Completed deal snapshot for benchmarking reference.</summary>
    public class RecentBenchmarkDto
    {
        public int OrderId { get; set; }
        public string ListingTitle { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string SellerCompany { get; set; } = string.Empty;
        public string BuyerCompany { get; set; } = string.Empty;
        public decimal AgreedUnitPrice { get; set; }
        public decimal AgreedQuantity { get; set; }
        public decimal TotalAmount { get; set; }
        public string BaseCurrency { get; set; } = "USD";
        public DateTime OrderDate { get; set; }
    }
}