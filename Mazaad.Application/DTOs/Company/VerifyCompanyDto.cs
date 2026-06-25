// Mazaad.Application/DTOs/Company/CompanyVerificationDto.cs

using System.ComponentModel.DataAnnotations;

namespace Mazaad.Application.DTOs.Company
{
    // بيتبعت من الـ SuperAdmin لما يوافق أو يرفض شركة
    public class VerifyCompanyDto
    {
        [Required]
        public bool Approved { get; set; }

        // مطلوب لو Approved = false
        public string? RejectionReason { get; set; }
    }

    // بيترجع للـ SuperAdmin في قائمة الشركات المنتظرة
    public class PendingCompanyDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string CommercialRegNum { get; set; } = string.Empty;
        public string TaxRegistrationNum { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string IndustryName { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
        public string AdminName { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
    }
}