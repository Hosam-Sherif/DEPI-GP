namespace Mazaad.Domain.Enums
{
    /// <summary>
    /// Defines the fulfillment lifecycle of an Order.
    ///
    /// Escrow-aware state machine:
    ///   Pending     → buyer has not yet initiated payment
    ///   Confirmed   → order terms agreed; payment not yet received (legacy / pre-escrow)
    ///   Processing  → Paymob payment confirmed; funds held in escrow by platform
    ///   Shipped     → seller has dispatched the goods
    ///   Delivered   → buyer has confirmed receipt; TRIGGERS automatic seller payout
    ///   Completed   → payout confirmed; order fully settled (terminal state)
    ///   Cancelled   → order cancelled; triggers buyer refund if escrow is Held
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>Buyer has not yet initiated payment.</summary>
        Pending = 0,

        /// <summary>Order terms agreed; awaiting payment (kept for backward compatibility).</summary>
        Confirmed = 1,

        /// <summary>
        /// Paymob has confirmed receipt of buyer funds. An EscrowRecord with
        /// Status = Held has been created. The seller should prepare the shipment.
        /// </summary>
        Processing = 2,

        /// <summary>The seller has dispatched the goods and provided tracking info.</summary>
        Shipped = 3,

        /// <summary>
        /// The buyer has confirmed delivery. This status transition automatically
        /// triggers EscrowService.ReleaseEscrowAsync, which initiates the seller payout.
        /// </summary>
        Delivered = 4,

        /// <summary>
        /// The seller payout has been confirmed by Paymob. The order is fully settled.
        /// Terminal state — no further transitions are allowed.
        /// </summary>
        Completed = 5,

        /// <summary>
        /// The order was cancelled. If an EscrowRecord with Status = Held exists,
        /// a buyer refund is automatically initiated via PaymentService.InitiateRefundAsync.
        /// </summary>
        Cancelled = 6
    }
}
