using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Mazaad.Domain.Enums;

namespace Mazaad.Domain.Models
{
    /// <summary>
    /// طلب شراء في المزاد المعكوس:
    /// شركة تعلن عن حاجتها لمادة خام والشركات الأخرى تتنافس بتقديم أفضل (أقل) سعر.
    /// </summary>
    public class ReverseAuction
    {
        [Key]
        public int Id { get; set; }

        // ── علاقة الشركة الطالبة ──────────────────────────────────────────────
        [ForeignKey(nameof(BuyerCompany))]
        public int BuyerCompanyId { get; set; }

        // ── علاقة الفئة ───────────────────────────────────────────────────────
        [ForeignKey(nameof(Category))]
        public int CategoryId { get; set; }

        // ── محتوى الطلب ───────────────────────────────────────────────────────
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        /// <summary>المواصفات الفنية المطلوبة (نص حر أو JSON)</summary>
        public string TechnicalSpecs { get; set; } = string.Empty;

        public decimal RequiredQuantity { get; set; }

        [MaxLength(20)]
        public string UnitOfMeasure { get; set; } = "Ton";

        /// <summary>الحد الأقصى للسعر الذي ترغب الشركة بدفعه لكل وحدة (السقف)</summary>
        public decimal? MaxBudgetPerUnit { get; set; }

        [MaxLength(10)]
        public string BaseCurrency { get; set; } = "USD";

        /// <summary>الموقع الجغرافي للتسليم</summary>
        [MaxLength(300)]
        public string DeliveryLocation { get; set; } = string.Empty;

        // ── التواريخ ──────────────────────────────────────────────────────────
        /// <summary>الموعد النهائي لتلقّي العروض</summary>
        public DateTime DeadlineDate { get; set; }

        // ── الحالة ────────────────────────────────────────────────────────────
        public ReverseAuctionStatus Status { get; set; } = ReverseAuctionStatus.Open;

        /// <summary>الـ offer الذي اختارته الشركة الطالبة (يُعيَّن عند Awarded)</summary>
        public int? AwardedOfferId { get; set; }

        // ── Audit ─────────────────────────────────────────────────────────────
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        // ── Navigations ───────────────────────────────────────────────────────
        public Companies BuyerCompany { get; set; } = null!;
        public Material_Categories Category { get; set; } = null!;
        public ICollection<ReverseAuctionOffer> Offers { get; set; } = new HashSet<ReverseAuctionOffer>();

        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}

