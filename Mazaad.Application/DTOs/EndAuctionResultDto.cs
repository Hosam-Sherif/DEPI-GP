using System;
using Mazaad.Domain.Enums;

namespace Mazaad.Application.DTOs
{
    /// <summary>
    /// Returned after ending an auction immediately (or when it ends naturally).
    /// Carries the outcome so the caller/UI can announce a winner right away.
    /// </summary>
    public class EndAuctionResultDto
    {
        public int ListingId { get; set; }
        public string Title { get; set; } = string.Empty;
        public ListingStatus Status { get; set; }
        public DateTime EndDate { get; set; }

        public bool HasWinner { get; set; }

        /// <summary>Null when HasWinner is false (no bids were placed).</summary>
        public int? WinningBidId { get; set; }
        public string? WinnerDisplayName { get; set; }
        public decimal? WinningBidAmountPerUnit { get; set; }
        public decimal? WinningTotalAmount { get; set; }
        public decimal? WinningQuantity { get; set; }
    }
}