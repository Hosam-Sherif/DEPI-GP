namespace Mazaad.Application.DTOs
{
    public class BidResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string DisplayBiddersName { get; set; } = string.Empty;
        public decimal NewPrice { get; set; }

        /// <summary>ID of the newly created bid; 0 if bid failed</summary>
        public int NewBidId { get; set; }

        /// <summary>Updated bid count on the listing</summary>
        public int NewBidCount { get; set; }
    }
}