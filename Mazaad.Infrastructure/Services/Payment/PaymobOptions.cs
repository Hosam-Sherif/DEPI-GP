namespace Mazaad.Infrastructure.Services.Payment
{
    public class PaymobOptions
    {
        public string ApiKey { get; set; } = string.Empty;

        // كارت أونلاين (الحالي)
        public string IntegrationId { get; set; } = string.Empty;
        public string IframeId { get; set; } = string.Empty;

        // محافظ إلكترونية
        public string VodafoneIntegrationId { get; set; } = string.Empty;
        public string OrangeIntegrationId { get; set; } = string.Empty;
        public string EtisalatIntegrationId { get; set; } = string.Empty;
        public string WePayIntegrationId { get; set; } = string.Empty;

        public string HmacSecret { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://accept.paymob.com";
    }
}