namespace Mazaad.Infrastructure.Services.Payout
{
    /// <summary>
    /// Configuration options for Paymob's Disbursement (Payout) API product.
    /// Loaded from appsettings.json under the "PaymobDisbursement" section.
    /// </summary>
    public class PaymobDisbursementOptions
    {
        /// <summary>
        /// The disbursement API Key. This is separate from the collection API Key
        /// as the disbursement product has separate merchant credentials on Paymob.
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Paymob's internal integration ID for disbursements.
        /// </summary>
        public string DisbursementIntegrationId { get; set; } = string.Empty;

        /// <summary>
        /// HMAC Secret Key used to verify incoming disbursement webhook payloads.
        /// </summary>
        public string HmacSecret { get; set; } = string.Empty;

        /// <summary>
        /// Base URL for Paymob API endpoints. Defaults to "https://accept.paymob.com".
        /// </summary>
        public string BaseUrl { get; set; } = "https://accept.paymob.com";
    }
}
