using System;

namespace Mazaad.Application.DTOs
{
    /// <summary>Company summary response DTO.</summary>
    public class CompanyResponseDto
    {
        public int Id { get; set; }
        public int IndustryId { get; set; }
        public string IndustryName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string CommercialRegNum { get; set; } = string.Empty;
        public string TaxRegistrationNum { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string AddressDetails { get; set; } = string.Empty;

        /// <summary>
        /// Pending / Verified / Rejected / Suspended — كـ string عشان الفرونت
        /// يستخدمها مباشرة في الـ template من غير ما يفك enum رقمي.
        /// </summary>
        public string VerificationStatus { get; set; } = string.Empty;

        /// <summary>
        /// سبب الرفض/التعليق — موجود لو الحالة Rejected أو Suspended، وبيكون null في باقي الحالات.
        /// </summary>
        public string? RejectionReason { get; set; }

        // باقي زي ما هو لحد ما الفرونت كله يتحول للـ VerificationStatus بدل IsVerified
        public bool IsVerified { get; set; }

        public DateTime CreatedAt { get; set; }

        // ── جديد: بيانات أول CompanyAdmin مرتبط بالشركة ────────────────────────
        public string? AdminFullName { get; set; }
        public string? AdminEmail { get; set; }
    }
}