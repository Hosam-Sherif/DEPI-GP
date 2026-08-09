using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Mazaad.Domain.Enums;

namespace Mazaad.Domain.Models
{
    /// <summary>
    /// Represents the platform's custody of buyer funds after a successful payment.
    ///
    /// Lifecycle:
    ///   1. Created (Status = Held) immediately after PaymentService confirms
    ///      a successful Paymob collection webhook.
    ///   2. Transitioned to Released when the Order reaches Delivered status
    ///      and a PayoutRecord is successfully initiated to the seller.
    ///   3. Transitioned to Refunded if the Order is cancelled post-payment.
    ///   4. Transitioned to Disputed if either party raises a dispute.
    ///
    /// IMPORTANT: This entity tracks INTERNAL fund state (custody), which is
    /// conceptually separate from the Payments entity that tracks the INBOUND
    /// Paymob transaction (money received from buyer). Never merge these two concerns.
    /// </summary>
    public class EscrowRecord
    {
        [Key]
        public int Id { get; set; }

        // ── Source Order ──────────────────────────────────────────────────────────

        /// <summary>
        /// The order whose buyer payment is being held.
        /// One-to-one: each Order has at most one active EscrowRecord.
        /// Configured as one-to-one in AppDbContext with the FK living here.
        /// </summary>
        [ForeignKey(nameof(Order))]
        public int OrderId { get; set; }

        // ── Source Payment ────────────────────────────────────────────────────────

        /// <summary>
        /// The specific Payments row whose successful Paymob transaction triggered
        /// the creation of this escrow. Stored here to provide a direct audit link:
        ///   Payments (inbound) → EscrowRecord (custody) → PayoutRecord (outbound).
        /// </summary>
        [ForeignKey(nameof(SourcePayment))]
        public int SourcePaymentId { get; set; }

        // ── Financial Snapshot ────────────────────────────────────────────────────

        /// <summary>
        /// The total amount collected from the buyer. Snapshot of Orders.TotalAmount
        /// at the moment the escrow was created. Immutable after creation.
        /// Precision configured as decimal(18,4) in AppDbContext.
        /// </summary>
        public decimal AmountHeld { get; set; }

        /// <summary>
        /// Snapshot of Orders.PlatformFee at escrow creation time.
        /// Stored explicitly to protect against future commission policy changes
        /// affecting historical records. Immutable after creation.
        /// </summary>
        public decimal PlatformFee { get; set; }

        /// <summary>
        /// Pre-computed: AmountHeld - PlatformFee.
        /// The exact amount the seller will receive via Paymob Disbursement.
        /// Stored (not computed on-the-fly) to ensure the payout amount is
        /// fixed at hold time and cannot drift. Immutable after creation.
        /// </summary>
        public decimal SellerDueAmount { get; set; }

        /// <summary>
        /// Currency of the held funds (e.g., "EGP"). Copied from the Payments row.
        /// </summary>
        [Required, MaxLength(10)]
        public string Currency { get; set; } = "EGP";

        // ── State ─────────────────────────────────────────────────────────────────

        public EscrowStatus Status { get; set; } = EscrowStatus.Held;

        // ── Timestamps ────────────────────────────────────────────────────────────

        /// <summary>When the escrow was created (payment confirmed).</summary>
        public DateTime HeldAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When the funds left the platform (Released or Refunded).
        /// Null while Status is Held or Disputed.
        /// </summary>
        public DateTime? ReleasedAt { get; set; }

        // ── Admin Notes ───────────────────────────────────────────────────────────

        /// <summary>
        /// Optional notes added by a SuperAdmin (e.g., dispute details,
        /// manual override reason).
        /// </summary>
        [MaxLength(1000)]
        public string? Notes { get; set; }

        // ── Navigation ────────────────────────────────────────────────────────────

        public Orders Order { get; set; } = null!;

        public Payments SourcePayment { get; set; } = null!;

        /// <summary>
        /// All payout attempts associated with this escrow hold.
        /// Under normal flow this collection has exactly one item with Status = Completed.
        /// Multiple items exist only when earlier attempts failed and were retried.
        /// </summary>
        public ICollection<PayoutRecord> Payouts { get; set; } = new HashSet<PayoutRecord>();
    }
}
