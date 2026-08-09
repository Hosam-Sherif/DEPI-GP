namespace Mazaad.Domain.Enums
{
    /// <summary>
    /// Defines the lifecycle of a PayoutRecord — a single disbursement attempt to a seller.
    /// Each failed attempt is kept immutable; retries create a new PayoutRecord.
    /// Transitions:
    ///   Created           →  Pending
    ///   Paymob API called →  Processing
    ///   Webhook confirmed →  Completed | Failed
    ///   Admin cancels     →  Cancelled  (only valid from Pending or Processing)
    /// </summary>
    public enum PayoutStatus
    {
        /// <summary>
        /// The PayoutRecord has been created but the Paymob Disbursement API
        /// has not yet been called. This state is transient and short-lived.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// The Paymob Disbursement API was called successfully and returned a
        /// disbursement ID. Waiting for Paymob's webhook to confirm the bank transfer.
        /// </summary>
        Processing = 1,

        /// <summary>
        /// Paymob's disbursement webhook confirmed that the bank transfer
        /// was executed. The seller has received the funds.
        /// </summary>
        Completed = 2,

        /// <summary>
        /// Paymob reported that the disbursement failed (e.g., invalid IBAN,
        /// account closed, bank rejection). A SuperAdmin can initiate a retry,
        /// which creates a new PayoutRecord — this record is never mutated.
        /// </summary>
        Failed = 3,

        /// <summary>
        /// The payout was manually cancelled by a SuperAdmin before Paymob
        /// processed the transfer. Only valid when Status is Pending.
        /// </summary>
        Cancelled = 4
    }
}
