using System;
using Mazaad.Domain.Enums;

namespace Mazaad.Application.DTOs.Escrow
{
    /// <summary>
    /// Lightweight escrow status summary embedded inside OrderResponseDto.
    /// Allows the frontend to show fund hold/release state on the order detail page
    /// without requiring a separate API call to EscrowController.
    ///
    /// Only contains the fields a buyer or seller actually needs on their order page —
    /// not the full financial breakdown, which is in EscrowRecordDto.
    /// </summary>
    public class EscrowStatusSummaryDto
    {
        /// <summary>Current escrow lifecycle state.</summary>
        public EscrowStatus Status { get; set; }

        /// <summary>Human-readable label for frontend display badges/chips.</summary>
        public string StatusLabel => Status switch
        {
            EscrowStatus.Held     => "Funds Held by Platform",
            EscrowStatus.Released => "Payment Released to Seller",
            EscrowStatus.Refunded => "Refunded to Buyer",
            EscrowStatus.Disputed => "Under Dispute — Contact Support",
            _                     => Status.ToString()
        };

        /// <summary>When the buyer's payment was confirmed and the hold was created.</summary>
        public DateTime HeldAt { get; set; }

        /// <summary>
        /// When the funds were released or refunded.
        /// Null while Status is Held or Disputed.
        /// </summary>
        public DateTime? ReleasedAt { get; set; }

        /// <summary>
        /// The amount the seller is due to receive once the order is delivered.
        /// Shown to the seller so they know exactly what payout to expect.
        /// </summary>
        public decimal SellerDueAmount { get; set; }

        /// <summary>Currency of the held/due amount.</summary>
        public string Currency { get; set; } = "EGP";
    }
}
