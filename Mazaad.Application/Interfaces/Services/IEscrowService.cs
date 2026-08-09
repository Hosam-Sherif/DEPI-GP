using System.Threading.Tasks;
using Mazaad.Application.DTOs.Escrow;

namespace Mazaad.Application.Interfaces.Services
{
    /// <summary>
    /// Manages the lifecycle of an EscrowRecord — the platform's custody of buyer funds
    /// between successful payment confirmation and physical delivery of goods.
    ///
    /// All write operations are triggered exclusively by internal system events
    /// (Paymob webhooks, order status transitions). There are NO direct API endpoints
    /// that mutate escrow state — this is intentional to prevent unauthorized fund releases.
    /// </summary>
    public interface IEscrowService
    {
        /// <summary>
        /// Creates an EscrowRecord with Status = Held, linking it to both the Order
        /// and the triggering Payment row.
        ///
        /// WHEN TO CALL: Immediately after PaymentService.HandlePaymobWebhookAsync confirms
        /// a successful collection (success = true). Must be called inside the same
        /// try/catch block as the payment status update, within a transaction.
        ///
        /// PRECONDITIONS:
        ///   - The Order identified by orderId must exist.
        ///   - The most recent Payments row for that order must have Status = Paid.
        ///   - No active (non-Refunded) EscrowRecord must already exist for this order
        ///     (idempotency guard against duplicate webhook deliveries).
        ///
        /// POSTCONDITIONS:
        ///   - A new EscrowRecord is persisted with AmountHeld, PlatformFee, and
        ///     SellerDueAmount snapshotted from the Order at the time of this call.
        ///   - Payments.EscrowRecordId is updated to reference the new record.
        ///   - Order.Status is NOT changed here — that is the caller's responsibility.
        /// </summary>
        /// <param name="orderId">The ID of the order whose payment just succeeded.</param>
        /// <param name="sourcePaymentId">The ID of the Payments row that triggered the escrow.</param>
        /// <returns>The newly created EscrowRecordDto.</returns>
        Task<EscrowRecordDto> CreateEscrowAsync(int orderId, int sourcePaymentId);

        /// <summary>
        /// Transitions the EscrowRecord to Released and initiates the seller payout.
        ///
        /// WHEN TO CALL: Inside OrderService.UpdateOrderStatusAsync, immediately after
        /// the order status is persisted as Delivered.
        ///
        /// BEHAVIOR ON PAYOUT INITIATION FAILURE:
        ///   The EscrowRecord status remains Held (NOT Released) if IPayoutService fails
        ///   to initiate the disbursement. A Telegram alert is sent to SuperAdmin for
        ///   manual intervention. The order status update (Delivered) still persists —
        ///   delivered goods cannot be "un-delivered" due to a financial API failure.
        ///
        /// PRECONDITIONS:
        ///   - An EscrowRecord with Status = Held must exist for this orderId.
        ///   - The seller company must have at least one SellerBankAccount where
        ///     IsVerified = true and IsDefault = true and IsDeleted = false.
        /// </summary>
        /// <param name="orderId">The ID of the order that just reached Delivered status.</param>
        Task ReleaseEscrowAsync(int orderId);

        /// <summary>
        /// Transitions the EscrowRecord to Refunded and initiates a buyer refund
        /// via Paymob's refund API.
        ///
        /// WHEN TO CALL: When an order with an active (Held) escrow is cancelled.
        /// This method delegates the actual Paymob API call to IPaymentService.InitiateRefundAsync.
        ///
        /// PRECONDITIONS:
        ///   - An EscrowRecord with Status = Held must exist for this orderId.
        ///   - A completed (Paid) payment with a valid ProviderTransactionId must exist.
        ///
        /// POSTCONDITIONS:
        ///   - EscrowRecord.Status is set to Refunded.
        ///   - A refund request is sent to Paymob against the original transaction.
        ///   - Buyer and seller receive in-app notifications.
        /// </summary>
        /// <param name="orderId">The ID of the cancelled order.</param>
        /// <param name="reason">Human-readable reason for the refund (stored in EscrowRecord.Notes).</param>
        Task RefundEscrowAsync(int orderId, string reason);

        /// <summary>
        /// Retrieves the EscrowRecord for a given order.
        /// Returns null if no escrow exists (payment not yet confirmed).
        /// </summary>
        Task<EscrowRecordDto?> GetEscrowForOrderAsync(int orderId);

        /// <summary>
        /// Returns a paginated list of all EscrowRecords for SuperAdmin oversight,
        /// optionally filtered by EscrowStatus.
        /// </summary>
        Task<IEnumerable<EscrowRecordDto>> GetAllEscrowsAsync(
            Domain.Enums.EscrowStatus? statusFilter = null);
    }
}
