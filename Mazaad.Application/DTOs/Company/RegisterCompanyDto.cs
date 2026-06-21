// Mazaad.Application/DTOs/Company/RegisterCompanyDto.cs

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Mazaad.Application.DTOs.Company
{
    public class RegisterCompanyDto
    {
        // ── بيانات الشركة ─────────────────────────────
        [Required]
        public int IndustryId { get; set; }

        [Required, MaxLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string CommercialRegNum { get; set; } = string.Empty;

        [MaxLength(100)]
        public string TaxRegistrationNum { get; set; } = string.Empty;

        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        public string AddressDetails { get; set; } = string.Empty;

        // ── المستندات ─────────────────────────────────
        /// <summary>
        /// السجل التجاري — مطلوب
        /// </summary>
        [Required]
        public IFormFile CommercialRegisterDocument { get; set; } = null!;

        /// <summary>
        /// البطاقة الضريبية — مطلوب
        /// </summary>
        [Required]
        public IFormFile TaxCardDocument { get; set; } = null!;

        /// <summary>
        /// مستندات إضافية اختيارية
        /// </summary>
        public List<IFormFile>? AdditionalDocuments { get; set; }

        // ── بيانات الـ Admin الأول للشركة ─────────────
        [Required, MaxLength(200)]
        public string AdminFullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string AdminEmail { get; set; } = string.Empty;

        [Required, MinLength(8)]
        public string AdminPassword { get; set; } = string.Empty;

        [Required, Compare(nameof(AdminPassword))]
        public string ConfirmPassword { get; set; } = string.Empty;

        [MaxLength(100)]
        public string AdminJobTitle { get; set; } = string.Empty;
    }
}