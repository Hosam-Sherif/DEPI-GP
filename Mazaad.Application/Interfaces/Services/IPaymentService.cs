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
        /// Returns false if the HMAC signature doesn't match (request rejected).
        /// </summary>
        Task<bool> HandlePaymobWebhookAsync(JsonElement payload, string hmacFromQuery);
    }
}