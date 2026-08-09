using System.Text.Json;
using System.Threading.Tasks;
using Mazaad.Application.DTOs;

namespace Mazaad.Application.Interfaces.Services
{
    public interface IPaymentService
    {
        /// <summary>
        /// Creates a Payments row for the given order (if one isn't already pending),
        /// then calls Paymob (auth -> order register -> payment key) and returns
        /// the iframe URL the frontend should open/redirect to.
        /// </summary>
        Task<PaymentInitiationResponseDto> InitiatePaymentAsync(int companyId, CreatePaymentRequestDto request);

        Task<PaymentResponseDto?> GetPaymentForOrderAsync(int orderId);

        /// <summary>
        /// Handles the "Transaction Processed" webhook Paymob POSTs after the
        /// customer finishes the payment flow. Verifies the HMAC signature,
        /// then updates Payments + Orders status accordingly.
        ///
        /// ESCROW CHANGE: On payment success, this method must now set the order
        /// to OrderStatus.Processing (NOT Completed) and call
        /// IEscrowService.CreateEscrowAsync to lock the funds in escrow.
        ///
        /// Returns false if the HMAC signature doesn't match (request rejected).
        /// </summary>
        Task<bool> HandlePaymobWebhookAsync(JsonElement payload, string hmacFromQuery);

        /// <summary>
        /// Initiates a buyer refund via Paymob's refund API by reversing the original
        /// collection transaction.
        ///
        /// WHEN TO CALL: From IEscrowService.RefundEscrowAsync when an order that
        /// has a Held EscrowRecord is cancelled. Must NOT be called directly from a
        /// controller; escrow integrity checks must run first.
        ///
        /// PRECONDITIONS:
        ///   - A Payments row for this order must have Status = Paid and a valid
        ///     ProviderTransactionId (the original Paymob transaction to reverse).
        ///
        /// POSTCONDITIONS:
        ///   - Paymob's refund endpoint is called with the original transaction ID.
        ///   - Payments.Status is set to Refunded.
        ///   - The buyer receives an in-app notification.
        ///
        /// NOTE: This method does NOT change EscrowRecord.Status — that is the
        /// responsibility of IEscrowService.RefundEscrowAsync, which is the caller.
        /// </summary>
        /// <param name="orderId">The ID of the order being refunded.</param>
        /// <param name="reason">Human-readable refund reason for internal records.</param>
        Task<bool> InitiateRefundAsync(int orderId, string reason);
    }
}