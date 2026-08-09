using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Mazaad.Infrastructure.Services.Payout
{
    /// <summary>
    /// Specialized HTTP Client for interacting with Paymob's Disbursement (Payout) API.
    /// Keep separate from the collection client to isolate credentials and API models.
    /// </summary>
    public class PaymobDisbursementClient
    {
        private readonly HttpClient _http;
        private readonly PaymobDisbursementOptions _options;

        public PaymobDisbursementClient(HttpClient http, IOptions<PaymobDisbursementOptions> options)
        {
            _http = http;
            _options = options.Value;
            _http.BaseAddress = new Uri(_options.BaseUrl);
        }

        /// <summary>
        /// Authenticates with Paymob using the disbursement API key.
        /// Returns the JWT bearer token for subsequent calls.
        /// </summary>
        public async Task<string> AuthenticateAsync()
        {
            var response = await _http.PostAsJsonAsync("/api/auth/tokens", new
            {
                api_key = _options.ApiKey
            });

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            return json.GetProperty("token").GetString()!;
        }

        /// <summary>
        /// Disburses funds to a seller's bank account or mobile wallet.
        /// </summary>
        /// <param name="authToken">Disbursement auth token obtained from AuthenticateAsync.</param>
        /// <param name="amountCents">The seller's due amount in cents (e.g. EGP 100.00 = 10000 cents).</param>
        /// <param name="currency">Currency code (e.g., "EGP").</param>
        /// <param name="accountType">Type of account: BankTransfer or MobileWallet.</param>
        /// <param name="accountHolderName">The legal name on the destination account.</param>
        /// <param name="bankCode">Required if BankTransfer (Paymob's internal bank identifier).</param>
        /// <param name="accountNumber">Required if BankTransfer (destination account number).</param>
        /// <param name="iban">Optional if BankTransfer (IBAN format).</param>
        /// <param name="mobileWalletNumber">Required if MobileWallet (Egyptian 11-digit number starting with 01).</param>
        /// <returns>A tuple containing (PaymobDisbursementId, PaymobDisbursementRef).</returns>
        public async Task<(string DisbursementId, string DisbursementRef)> CreateDisbursementAsync(
            string authToken,
            long amountCents,
            string currency,
            Domain.Enums.PayoutAccountType accountType,
            string accountHolderName,
            string? bankCode,
            string? accountNumber,
            string? iban,
            string? mobileWalletNumber)
        {
            // Build request payload according to Paymob Disbursement spec
            object disbursementPayload;

            if (accountType == Domain.Enums.PayoutAccountType.BankTransfer)
            {
                disbursementPayload = new
                {
                    amount_cents = amountCents,
                    integration_id = long.Parse(_options.DisbursementIntegrationId),
                    currency = currency,
                    payment_method = "BANK_TRANSFER",
                    receiver_details = new
                    {
                        full_name = accountHolderName,
                        bank_code = bankCode,
                        account_number = accountNumber,
                        iban = iban ?? string.Empty
                    }
                };
            }
            else // MobileWallet
            {
                disbursementPayload = new
                {
                    amount_cents = amountCents,
                    integration_id = long.Parse(_options.DisbursementIntegrationId),
                    currency = currency,
                    payment_method = "MOBILE_WALLET",
                    receiver_details = new
                    {
                        full_name = accountHolderName,
                        wallet_number = mobileWalletNumber
                    }
                };
            }

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/disbursement/disbursements")
            {
                Content = JsonContent.Create(disbursementPayload)
            };

            // Set Bearer authentication header
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            
            // Extract the disbursement ID and external transaction reference returned by Paymob
            var disbursementId = json.GetProperty("id").GetRawText();
            var transactionRef = json.TryGetProperty("transaction_reference", out var refProp) 
                ? refProp.GetString() ?? disbursementId 
                : disbursementId;

            return (disbursementId, transactionRef);
        }

        /// <summary>
        /// Verifies the HMAC signature of incoming disbursement webhooks.
        /// </summary>
        public bool VerifyWebhookSignature(JsonElement obj, string receivedHmac)
        {
            // Helper method to extract properties safely
            string Get(string name) => obj.TryGetProperty(name, out var v)
                ? v.ValueKind switch
                {
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    JsonValueKind.Null => "",
                    _ => v.ToString()
                }
                : "";

            // Concatenate ordered fields as specified in Paymob's webhook security documentation
            var ordered = string.Concat(
                Get("amount_cents"),
                Get("created_at"),
                Get("currency"),
                Get("error_occured"),
                Get("id"),
                Get("integration_id"),
                Get("success")
            );

            var keyBytes = Encoding.UTF8.GetBytes(_options.HmacSecret);
            var messageBytes = Encoding.UTF8.GetBytes(ordered);

            using var hmacSha512 = new HMACSHA512(keyBytes);
            var hash = hmacSha512.ComputeHash(messageBytes);
            var computedHex = Convert.ToHexString(hash).ToLowerInvariant();

            return string.Equals(computedHex, receivedHmac, StringComparison.OrdinalIgnoreCase);
        }
    }
}
