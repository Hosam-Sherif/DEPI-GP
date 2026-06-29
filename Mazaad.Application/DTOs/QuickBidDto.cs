namespace Mazaad.Application.DTOs
{
    public class QuickBidDto
    {
        public int ListingId { get; set; }
        public decimal BidAmountPerUnit { get; set; }
        public bool IsAnonymous { get; set; }
    }
}