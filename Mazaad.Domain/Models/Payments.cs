using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Mazaad.Domain.Enums;


namespace Mazaad.Domain.Models
{
    public class Payments
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EGP";

        /// <summary>"Paymob", "Cash", etc.</summary>
        public string PaymentMethod { get; set; } = string.Empty;

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        // ─── Paymob-specific tracking fields ────────────────────────────────────

        /// <summary>The order id created on Paymob's side (api/ecommerce/orders).</summary>
        public string? ProviderOrderId { get; set; }

        /// <summary>The transaction id Paymob sends back in the webhook callback.</summary>
        public string? ProviderTransactionId { get; set; }

        /// <summary>The payment_key token used to build the iframe URL for this attempt.</summary>
        public string? PaymentToken { get; set; }

        public string TransactionReference { get; set; } = string.Empty;

        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ── Escrow Link ─────────────────────────────────────────────────────

        /// <summary>
        /// The EscrowRecord that was created when this payment was confirmed.
        /// Null if the payment failed or the escrow has not yet been created.
        ///
        /// This FK creates the direct audit chain:
        ///   Payments (buyer inbound) → EscrowRecord (platform custody) → PayoutRecord (seller outbound).
        ///
        /// Configured with DeleteBehavior.NoAction in AppDbContext: deleting a payment
        /// must NOT cascade-delete the EscrowRecord or any associated PayoutRecords.
        /// </summary>
        [ForeignKey(nameof(Escrow))]
        public int? EscrowRecordId { get; set; }

        public Orders Order { get; set; } = null!;

        /// <summary>The escrow hold created when this payment was confirmed. Null until payment succeeds.</summary>
        public EscrowRecord? Escrow { get; set; }
    }
}