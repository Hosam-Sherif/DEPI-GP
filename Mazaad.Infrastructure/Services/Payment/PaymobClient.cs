using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Mazaad.Application.DTOs;
using Microsoft.Extensions.Options;

namespace Mazaad.Infrastructure.Services.Payment
{
    public class PaymobClient
    {
        private readonly HttpClient _http;
        private readonly PaymobOptions _options;

        public PaymobClient(HttpClient http, IOptions<PaymobOptions> options)
        {
            _http = http;
            _options = options.Value;
            _http.BaseAddress = new System.Uri(_options.BaseUrl);
        }

        // ─── Step 1: Auth Token ───────────────────────────────────────────────

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

        // ─── Step 2: Register Order ───────────────────────────────────────────

        public async Task<string> RegisterOrderAsync(
            string authToken, int merchantOrderId, long amountCents, string currency)
        {
            var response = await _http.PostAsJsonAsync("/api/ecommerce/orders", new
            {
                auth_token = authToken,
                delivery_needed = false,
                amount_cents = amountCents,
                currency,
                merchant_order_id = merchantOrderId.ToString(),
                items = System.Array.Empty<object>()
            });

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            return json.GetProperty("id").GetRawText();
        }

        // ─── Step 3a: Payment Key — Card (iframe) ─────────────────────────────

        public async Task<string> RequestCardPaymentKeyAsync(
            string authToken, string paymobOrderId, long amountCents, string currency,
            string buyerEmail, string buyerPhone, string buyerFirstName, string buyerLastName)
        {
            var response = await _http.PostAsJsonAsync("/api/acceptance/payment_keys", new
            {
                auth_token = authToken,
                amount_cents = amountCents,
                expiration = 3600,
                order_id = paymobOrderId,
                currency,
                integration_id = long.Parse(_options.IntegrationId),
                billing_data = BuildBillingData(buyerEmail, buyerPhone, buyerFirstName, buyerLastName)
            });

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            return json.GetProperty("token").GetString()!;
        }

        // ─── Step 3b: Payment Key — Mobile Wallet ─────────────────────────────

        public async Task<WalletPaymentResult> RequestWalletPaymentAsync(
            string authToken, string paymobOrderId, long amountCents, string currency,
            string buyerEmail, string buyerPhone, string buyerFirstName, string buyerLastName,
            string walletMobileNumber, PaymentMethodType method)
        {
            var integrationId = GetWalletIntegrationId(method);

            // Step 3b-i: الحصول على payment key
            var keyResponse = await _http.PostAsJsonAsync("/api/acceptance/payment_keys", new
            {
                auth_token = authToken,
                amount_cents = amountCents,
                expiration = 3600,
                order_id = paymobOrderId,
                currency,
                integration_id = long.Parse(integrationId),
                billing_data = BuildBillingData(buyerEmail, buyerPhone, buyerFirstName, buyerLastName)
            });

            keyResponse.EnsureSuccessStatusCode();
            var keyJson = await keyResponse.Content.ReadFromJsonAsync<JsonElement>();
            var paymentToken = keyJson.GetProperty("token").GetString()!;

            // Step 3b-ii: إرسال طلب الدفع (Paymob بيبعت OTP للعميل)
            var walletResponse = await _http.PostAsJsonAsync("/api/acceptance/payments/pay", new
            {
                source = new
                {
                    identifier = walletMobileNumber,
                    subtype = "WALLET"
                },
                payment_token = paymentToken
            });

            walletResponse.EnsureSuccessStatusCode();
            var walletJson = await walletResponse.Content.ReadFromJsonAsync<JsonElement>();

            var redirectUrl = walletJson.TryGetProperty("redirect_url", out var r)
                ? r.GetString()
                : null;

            return new WalletPaymentResult
            {
                PaymentToken = paymentToken,
                RedirectUrl = redirectUrl
            };
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        public string BuildIframeUrl(string paymentToken)
            => $"{_options.BaseUrl}/api/acceptance/iframes/{_options.IframeId}?payment_token={paymentToken}";

        private string GetWalletIntegrationId(PaymentMethodType method) => method switch
        {
            PaymentMethodType.VodafoneCash => _options.VodafoneIntegrationId,
            PaymentMethodType.OrangeMoney => _options.OrangeIntegrationId,
            PaymentMethodType.EtisalatCash => _options.EtisalatIntegrationId,
            PaymentMethodType.WePay => _options.WePayIntegrationId,
            _ => throw new System.InvalidOperationException($"Unknown wallet method: {method}")
        };

        private static object BuildBillingData(
            string email, string phone, string firstName, string lastName) => new
            {
                apartment = "NA",
                email,
                floor = "NA",
                first_name = firstName,
                street = "NA",
                building = "NA",
                phone_number = phone,
                shipping_method = "NA",
                postal_code = "NA",
                city = "Cairo",
                country = "EG",
                last_name = lastName,
                state = "NA"
            };
    }

    public class WalletPaymentResult
    {
        public string PaymentToken { get; set; } = string.Empty;
        public string? RedirectUrl { get; set; }
    }
}