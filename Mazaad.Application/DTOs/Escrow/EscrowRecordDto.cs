using System;
using Mazaad.Domain.Enums;

namespace Mazaad.Application.DTOs.Escrow
{
    /// <summary>
    /// Full read-only response DTO for an EscrowRecord.
    /// Returned by IEscrowService and exposed via EscrowController.
    /// Contains the complete financial snapshot and lifecycle timestamps
    /// needed for both company dashboards and SuperAdmin oversight.
    /// </summary>
    public class EscrowRecordDto
    {
        public int Id { get; set; }

        /// <summary>The Order this escrow is associated with.</summary>
        public int OrderId { get; set; }

        /// <summary>The Payments row whose success triggered this escrow.</summary>
        public int SourcePaymentId { get; set; }

        // ── Financial Snapshot ────────────────────────────────────────────────

        /// <summary>Total amount collected from the buyer (snapshot of Orders.TotalAmount).</summary>
        public decimal AmountHeld { get; set; }

        /// <summary>Platform commission (snapshot of Orders.PlatformFee).</summary>
        public decimal PlatformFee { get; set; }

        /// <summary>Amount the seller will receive: AmountHeld - PlatformFee.</summary>
        public decimal SellerDueAmount { get; set; }

        /// <summary>Currency of held funds (e.g., "EGP").</summary>
        public string Currency { get; set; } = "EGP";

        // ── State ─────────────────────────────────────────────────────────────

        /// <summary>Current escrow lifecycle state.</summary>
        public EscrowStatus Status { get; set; }

        /// <summary>Human-readable status label for frontend display.</summary>
        public string StatusLabel => Status switch
        {
            EscrowStatus.Held     => "Funds Held",
            EscrowStatus.Released => "Funds Released to Seller",
            EscrowStatus.Refunded => "Refunded to Buyer",
            EscrowStatus.Disputed => "Under Dispute",
            _                     => Status.ToString()
        };

        // ── Timestamps ────────────────────────────────────────────────────────

        /// <summary>When the escrow was created (payment confirmed by Paymob).</summary>
        public DateTime HeldAt { get; set; }

        /// <summary>When funds left the platform (Released or Refunded). Null while Held or Disputed.</summary>
        public DateTime? ReleasedAt { get; set; }

        // ── Admin ─────────────────────────────────────────────────────────────

        /// <summary>Optional SuperAdmin notes (dispute details, manual override reason).</summary>
        public string? Notes { get; set; }
    }
}
