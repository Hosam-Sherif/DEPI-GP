using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Mazaad.Application.DTOs.Payout;
using Mazaad.Domain.Enums;

namespace Mazaad.Application.Interfaces.Services
{
    /// <summary>
    /// Manages the lifecycle of PayoutRecord entities — individual Paymob Disbursement
    /// attempts that transfer the seller's due amount from the platform's merchant account
    /// to the seller's registered bank account or mobile wallet.
    ///
    /// IMMUTABILITY GUARANTEE: Once a PayoutRecord reaches a terminal state
    /// (Completed, Failed, Cancelled), it is NEVER mutated. Retries produce NEW
    /// PayoutRecord rows, preserving a complete, tamper-proof audit history.
    /// </summary>
    public interface IPayoutService
    {
        /// <summary>
        /// Creates a PayoutRecord and calls the Paymob Disbursement API to transfer
        /// SellerDueAmount to the seller's default verified bank account or wallet.
        ///
        /// WHEN TO CALL: From IEscrowService.ReleaseEscrowAsync, after the EscrowRecord
        /// status has been verified as Held. Do NOT call this directly from a controller.
        ///
        /// STEPS PERFORMED:
        ///   1. Loads the EscrowRecord and its parent Order to identify the seller.
        ///   2. Finds the seller's default verified SellerBankAccount
        ///      (IsDefault = true, IsVerified = true, IsDeleted = false).
        ///      Throws if none is found — surfaced as an alert to SuperAdmin.
        ///   3. Creates a PayoutRecord with Status = Pending.
        ///   4. Calls PaymobDisbursementClient to POST the disbursement.
        ///   5. Updates PayoutRecord.PaymobDisbursementId and sets Status = Processing.
        ///   6. Persists all changes in a single SaveChangesAsync call.
        /// </summary>
        /// <param name="escrowRecordId">ID of the EscrowRecord whose funds are being disbursed.</param>
        /// <returns>The created PayoutRecordDto with Status = Processing.</returns>
        Task<PayoutRecordDto> InitiatePayoutAsync(int escrowRecordId);

        /// <summary>
        /// Processes a Paymob disbursement webhook callback.
        ///
        /// WHEN TO CALL: From PayoutsController.HandleDisbursementWebhook, which is the
        /// public endpoint Paymob posts to when a bank transfer completes or fails.
        ///
        /// STEPS PERFORMED:
        ///   1. Verifies the HMAC signature using PaymobDisbursementOptions.HmacSecret.
        ///      Returns false immediately if invalid — the controller must return 401.
        ///   2. Extracts the disbursement ID from the payload.
        ///   3. Finds the PayoutRecord with matching PaymobDisbursementId.
        ///   4. If success = true:
        ///        - Sets Status = Completed, CompletedAt = now.
        ///        - Sets EscrowRecord.Status = Released, ReleasedAt = now.
        ///        - Sets Order.Status = Completed.
        ///        - Sends an in-app notification to the seller.
        ///   5. If success = false:
        ///        - Sets Status = Failed, FailureReason from payload.
        ///        - Sends a Telegram alert to SuperAdmin for manual retry.
        ///        - Does NOT change EscrowRecord.Status (stays Held for retry).
        ///   6. Persists all changes in a transaction.
        /// </summary>
        /// <returns>True if signature is valid and processing succeeded; false to reject the webhook.</returns>
        Task<bool> HandlePayoutWebhookAsync(JsonElement payload, string signatureFromHeader);

        /// <summary>
        /// Creates a new PayoutRecord and re-initiates disbursement for a previously
        /// failed payout. The original failed PayoutRecord is left untouched.
        ///
        /// PRECONDITIONS:
        ///   - The PayoutRecord identified by payoutRecordId must have Status = Failed.
        ///   - The linked EscrowRecord must still have Status = Held.
        ///   - Only a SuperAdmin can call this (enforced at controller level).
        /// </summary>
        /// <param name="payoutRecordId">ID of the failed PayoutRecord to retry.</param>
        /// <returns>A new PayoutRecordDto for the retry attempt.</returns>
        Task<PayoutRecordDto> RetryPayoutAsync(int payoutRecordId);

        /// <summary>
        /// Returns all payout records for a specific seller company, ordered by
        /// InitiatedAt descending. Used for the seller's payout history dashboard.
        /// </summary>
        Task<IEnumerable<PayoutRecordDto>> GetPayoutsForSellerAsync(int sellerCompanyId);

        /// <summary>
        /// Returns all payout records platform-wide, optionally filtered by status.
        /// SuperAdmin only.
        /// </summary>
        Task<IEnumerable<PayoutRecordDto>> GetAllPayoutsAsync(PayoutStatus? statusFilter = null);

        /// <summary>
        /// Returns the detail of a single PayoutRecord by its ID.
        /// Returns null if not found.
        /// </summary>
        Task<PayoutRecordDto?> GetPayoutByIdAsync(int payoutRecordId);
    }
}
