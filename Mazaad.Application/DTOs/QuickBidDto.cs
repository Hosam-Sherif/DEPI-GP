namespace Mazaad.Application.DTOs
{
    /// <summary>One-click Quick Bid from the marketplace card — uses listing's MinOrderQuantity automatically.</summary>
    public class QuickBidDto
    {
        public int ListingId { get; set; }

        /// <summary>Bid amount per unit. Must exceed CurrentHighestBid.</summary>
        public decimal BidAmountPerUnit { get; set; }

        public bool IsAnonymous { get; set; }
    }
}
