namespace Mazaad.Application.DTOs
{
    /// <summary>
    /// بيانات الشركة المعروضة للعموم في صفحة /companies.
    /// لا تحتوي على بيانات حساسة (رقم ضريبي، بريد الأدمن، إلخ).
    /// </summary>
    public class CompanyPublicDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string IndustryName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;

        /// <summary>عدد المزادات النشطة الخاصة بالشركة.</summary>
        public int ActiveListingsCount { get; set; }

        /// <summary>تاريخ التوثيق.</summary>
        public DateTime VerifiedAt { get; set; }
    }
}