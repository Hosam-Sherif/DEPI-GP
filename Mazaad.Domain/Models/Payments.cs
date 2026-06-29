using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        public Orders Order { get; set; } = null!;
    }
}