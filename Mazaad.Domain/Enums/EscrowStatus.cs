namespace Mazaad.Domain.Enums
{
    /// <summary>
    /// Defines the lifecycle of an EscrowRecord — the platform's internal hold on buyer funds.
    /// Transitions:
    ///   Payment confirmed  →  Held
    ///   Order Delivered    →  Released  (seller payout succeeded)
    ///   Order Cancelled    →  Refunded  (buyer refund issued via Paymob)
    ///   Dispute raised     →  Disputed  (funds frozen; admin review required)
    /// </summary>
    public enum EscrowStatus
    {
        /// <summary>
        /// Funds have been collected from the buyer and are held by the platform.
        /// Waiting for the order to reach Delivered status.
        /// </summary>
        Held = 0,

        /// <summary>
        /// A successful payout to the seller was confirmed by Paymob.
        /// The seller received TotalAmount minus PlatformFee.
        /// </summary>
        Released = 1,

        /// <summary>
        /// The order was cancelled after payment. A refund to the buyer
        /// has been initiated via Paymob's refund API.
        /// </summary>
        Refunded = 2,

        /// <summary>
        /// A dispute has been raised by either party. Funds are frozen
        /// and cannot be released or refunded until a SuperAdmin resolves the dispute.
        /// </summary>
        Disputed = 3
    }
}
