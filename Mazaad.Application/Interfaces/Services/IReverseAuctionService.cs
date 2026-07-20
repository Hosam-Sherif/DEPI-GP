using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mazaad.Application.Common;
using Mazaad.Application.DTOs;
using Mazaad.Application.DTOs.ReverseAuction;

namespace Mazaad.Application.Interfaces.Services
{
    /// <summary>
    /// خدمة المزاد المعكوس:
    /// تُدير دورة حياة طلبات الشراء وعروض الموردين
    /// </summary>
    public interface IReverseAuctionService
    {
        // ── Buyer: إدارة الطلبات ───────────────────────────────────────────────

        /// <summary>إنشاء طلب شراء جديد</summary>
        Task<ReverseAuctionDetailDto> CreateAsync(int buyerCompanyId, CreateReverseAuctionDto dto);

        /// <summary>قائمة الطلبات المفتوحة للعموم مع فلترة وتصفح</summary>
        Task<PagedResultDto<ReverseAuctionCardDto>> GetAllAsync(ReverseAuctionFilterDto filter);

        /// <summary>تفاصيل طلب شراء بالمعرِّف</summary>
        Task<ReverseAuctionDetailDto?> GetByIdAsync(int id, int? viewerCompanyId = null);

        /// <summary>طلبات الشراء الخاصة بشركة (صاحبها)</summary>
        Task<PagedResultDto<ReverseAuctionCardDto>> GetMyRequestsAsync(int buyerCompanyId, ReverseAuctionFilterDto filter);

        /// <summary>إلغاء طلب شراء — فقط صاحبه ولا يوجد عروض مقبولة</summary>
        Task<Result> CancelAsync(int reverseAuctionId, int buyerCompanyId);

        // ── Supplier: تقديم العروض ─────────────────────────────────────────────

        /// <summary>
        /// تقديم عرض سعر من شركة مورّدة.
        /// قاعدة عمل: لا تستطيع الشركة الطالبة تقديم عرض على طلبها الخاص.
        /// قاعدة عمل: لا تستطيع الشركة تقديم أكثر من عرض واحد على نفس الطلب (يمكنها تحديث عرضها).
        /// </summary>
        Task<Result<ReverseAuctionOfferDto>> SubmitOfferAsync(int supplierCompanyId, CreateReverseAuctionOfferDto dto);

        /// <summary>عرض العروض المقدَّمة — مرئي فقط لصاحب الطلب أو SuperAdmin</summary>
        Task<IEnumerable<ReverseAuctionOfferDto>> GetOffersAsync(int reverseAuctionId, int requestingCompanyId);

        /// <summary>
        /// قبول عرض واحد وترسية الطلب على المورّد المختار.
        /// يُغلق الطلب ويُنشئ Order ويُبلَّغ جميع الموردين.
        /// </summary>
        Task<Result> AwardOfferAsync(int reverseAuctionId, int offerId, int buyerCompanyId);

        /// <summary>سحب عرض مقدَّم (من الشركة المورّدة نفسها)</summary>
        Task<Result> WithdrawOfferAsync(int offerId, int supplierCompanyId);

        /// <summary>العروض التي قدّمتها شركة مورّدة (سجلها)</summary>
        Task<IEnumerable<ReverseAuctionOfferDto>> GetMyOffersAsync(int supplierCompanyId);
    }
}

