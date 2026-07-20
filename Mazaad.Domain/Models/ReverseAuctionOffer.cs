using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mazaad.Domain.Models
{
    /// <summary>
    /// عرض سعر مقدَّم من شركة مورّدة على طلب شراء في المزاد المعكوس.
    /// </summary>
    public class ReverseAuctionOffer
    {
        [Key]
        public int Id { get; set; }

        // ── علاقة الطلب ───────────────────────────────────────────────────────
        [ForeignKey(nameof(ReverseAuction))]
        public int ReverseAuctionId { get; set; }

        // ── علاقة الشركة المورّدة ─────────────────────────────────────────────
        [ForeignKey(nameof(SupplierCompany))]
        public int SupplierCompanyId { get; set; }

        // ── تفاصيل العرض ─────────────────────────────────────────────────────
        public decimal PricePerUnit { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal OfferedQuantity { get; set; }

        /// <summary>شروط التسليم (مثال: FOB، CIF، EXW، ...)</summary>
        [MaxLength(500)]
        public string DeliveryTerms { get; set; } = string.Empty;

        /// <summary>مدة التسليم بالأيام</summary>
        public int? DeliveryDays { get; set; }

        /// <summary>ملاحظات إضافية من المورّد</summary>
        public string Notes { get; set; } = string.Empty;

        /// <summary>تم اختيار هذا العرض من قِبَل الشركة الطالبة</summary>
        public bool IsAwarded { get; set; }

        // ── Audit ─────────────────────────────────────────────────────────────
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // ── Navigations ───────────────────────────────────────────────────────
        public ReverseAuction ReverseAuction { get; set; } = null!;
        public Companies SupplierCompany { get; set; } = null!;
    }
}
