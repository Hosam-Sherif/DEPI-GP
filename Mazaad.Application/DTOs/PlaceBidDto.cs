namespace Mazaad.Application.DTOs
{
    public class PlaceBidDto
    {
        public int ListingId { get; set; }
        public decimal BidAmountPerUnit { get; set; }
        public decimal TotalBidAmount { get; set; }
        public decimal Quantity { get; set; }
        public bool IsAnonymous { get; set; }
    }
}