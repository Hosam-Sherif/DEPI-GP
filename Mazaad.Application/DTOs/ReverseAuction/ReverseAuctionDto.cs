using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mazaad.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Mazaad.Application.DTOs.ReverseAuction
{
    // ─── Create / Update ──────────────────────────────────────────────────────────

    /// <summary>بيانات إنشاء طلب شراء جديد في المزاد المعكوس</summary>
    public class CreateReverseAuctionDto
    {
        [Required(ErrorMessage = "CategoryId is required.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, ErrorMessage = "Title must not exceed 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = string.Empty;

        public string TechnicalSpecs { get; set; } = string.Empty;

        [Range(0.001, double.MaxValue, ErrorMessage = "Required quantity must be greater than zero.")]
        public decimal RequiredQuantity { get; set; }

        /// <summary>السعر الأقصى للوحدة (اختياري — يُستخدم كسقف للعروض)</summary>
        [Range(0.01, double.MaxValue, ErrorMessage = "Max budget per unit must be positive.")]
        public decimal? MaxBudgetPerUnit { get; set; }

        [Required(ErrorMessage = "Base currency is required.")]
        [StringLength(10, ErrorMessage = "Currency code must not exceed 10 characters.")]
        public string BaseCurrency { get; set; } = "USD";

        public string DeliveryLocation { get; set; } = string.Empty;

        [Required(ErrorMessage = "Deadline date is required.")]
        public DateTime DeadlineDate { get; set; }
    }

    // ─── Response DTOs ────────────────────────────────────────────────────────────

    /// <summary>بطاقة الطلب للعرض في القوائم</summary>
    public class ReverseAuctionCardDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string BuyerCompanyName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal RequiredQuantity { get; set; }
        public string UnitOfMeasure { get; set; } = string.Empty;
        public decimal? MaxBudgetPerUnit { get; set; }
        public string BaseCurrency { get; set; } = string.Empty;
        public string DeliveryLocation { get; set; } = string.Empty;
        public DateTime DeadlineDate { get; set; }
        public ReverseAuctionStatus Status { get; set; }
        public int OffersCount { get; set; }
        public decimal? LowestOfferPrice { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>تفاصيل طلب الشراء الكاملة (تشمل قائمة العروض المقدَّمة)</summary>
    public class ReverseAuctionDetailDto
    {
        public int Id { get; set; }
        public int BuyerCompanyId { get; set; }
        public string BuyerCompanyName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TechnicalSpecs { get; set; } = string.Empty;
        public decimal RequiredQuantity { get; set; }
        public string UnitOfMeasure { get; set; } = string.Empty;
        public decimal? MaxBudgetPerUnit { get; set; }
        public string BaseCurrency { get; set; } = string.Empty;
        public string DeliveryLocation { get; set; } = string.Empty;
        public DateTime DeadlineDate { get; set; }
        public ReverseAuctionStatus Status { get; set; }
        public int? AwardedOfferId { get; set; }
        public int OffersCount { get; set; }
        public decimal? LowestOfferPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        /// <summary>مرئي فقط لصاحب الطلب</summary>
        public IEnumerable<ReverseAuctionOfferDto>? Offers { get; set; }
    }

    // ─── Offer DTOs ───────────────────────────────────────────────────────────────

    /// <summary>بيانات تقديم عرض سعر جديد على طلب شراء</summary>
    public class CreateReverseAuctionOfferDto
    {
        [Required]
        public int ReverseAuctionId { get; set; }

        [Range(0.0001, double.MaxValue, ErrorMessage = "Price per unit must be greater than zero.")]
        public decimal PricePerUnit { get; set; }

        [Range(0.001, double.MaxValue, ErrorMessage = "Offered quantity must be greater than zero.")]
        public decimal OfferedQuantity { get; set; }

        [StringLength(500, ErrorMessage = "Delivery terms must not exceed 500 characters.")]
        public string DeliveryTerms { get; set; } = string.Empty;

        [Range(1, 3650, ErrorMessage = "Delivery days must be between 1 and 3650.")]
        public int? DeliveryDays { get; set; }

        [StringLength(2000, ErrorMessage = "Notes must not exceed 2000 characters.")]
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>تفاصيل عرض سعر مقدَّم من مورّد</summary>
    public class ReverseAuctionOfferDto
    {
        public int Id { get; set; }
        public int ReverseAuctionId { get; set; }
        public int SupplierCompanyId { get; set; }
        public string SupplierCompanyName { get; set; } = string.Empty;
        public decimal PricePerUnit { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal OfferedQuantity { get; set; }
        public string DeliveryTerms { get; set; } = string.Empty;
        public int? DeliveryDays { get; set; }
        public string Notes { get; set; } = string.Empty;
        public bool IsAwarded { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ─── Filter ───────────────────────────────────────────────────────────────────

    /// <summary>معاملات الفلترة والتصفح لقائمة الطلبات</summary>
    public class ReverseAuctionFilterDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 9;
        public int? CategoryId { get; set; }
        public ReverseAuctionStatus? Status { get; set; }
        public string? SearchTerm { get; set; }
        public string? BaseCurrency { get; set; }
    }
}
