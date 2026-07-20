using Mazaad.Application.DTOs.ReverseAuction;
using Mazaad.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Mazaad.API.Controllers
{
    /// <summary>
    /// إدارة المزاد المعكوس (طلبات الشراء).
    ///
    /// المزاد المعكوس هو عملية يُعلن فيها المشتري عن احتياجاته
    /// ويتنافس الموردون بتقديم أفضل (أقل) سعر.
    /// </summary>
    [ApiController]
    [Route("api/reverse-auction")]
    public class ReverseAuctionController : ControllerBase
    {
        private readonly IReverseAuctionService _service;

        public ReverseAuctionController(IReverseAuctionService service)
        {
            _service = service;
        }

        // ══════════════════════════════════════════════════════════════════════
        // BUYER — إدارة طلبات الشراء
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// إنشاء طلب شراء جديد في المزاد المعكوس.
        /// فقط الشركات المُتحقّق منها يمكنها نشر طلبات.
        /// </summary>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ReverseAuctionDetailDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Create([FromBody] CreateReverseAuctionDto dto)
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Unauthorized(new { message = "A verified company account is required to post purchase requests." });

            try
            {
                var created = await _service.CreateAsync(companyId.Value, dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// قائمة طلبات الشراء المفتوحة (للجمهور — لا يحتاج مصادقة).
        /// يدعم الفلترة بالفئة، الحالة، والبحث النصي.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetAll([FromQuery] ReverseAuctionFilterDto filter)
        {
            var result = await _service.GetAllAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// تفاصيل طلب شراء محدد.
        /// صاحب الطلب يرى العروض المقدَّمة — الآخرون يرون البيانات العامة فقط.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ReverseAuctionDetailDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            var viewerCompanyId = GetCurrentCompanyId();
            var detail = await _service.GetByIdAsync(id, viewerCompanyId);
            if (detail == null) return NotFound(new { message = "Purchase request not found." });
            return Ok(detail);
        }

        /// <summary>
        /// طلبات الشراء الخاصة بالشركة الحالية (Buyer dashboard).
        /// </summary>
        [HttpGet("mine")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetMine([FromQuery] ReverseAuctionFilterDto filter)
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Unauthorized(new { message = "A verified company account is required." });

            var result = await _service.GetMyRequestsAsync(companyId.Value, filter);
            return Ok(result);
        }

        /// <summary>
        /// إلغاء طلب شراء.
        /// فقط صاحب الطلب يمكنه الإلغاء، ولا يمكن إلغاء طلب تم ترسيته.
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Cancel(int id)
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Unauthorized(new { message = "A verified company account is required." });

            var result = await _service.CancelAsync(id, companyId.Value);
            if (!result.Succeeded)
                return BadRequest(new { message = result.Error });

            return NoContent();
        }

        // ══════════════════════════════════════════════════════════════════════
        // SUPPLIER — تقديم وإدارة العروض
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// تقديم عرض سعر على طلب شراء.
        ///
        /// القواعد:
        /// - الشركة الطالبة لا تستطيع تقديم عرض على طلبها الخاص
        /// - كل شركة تقدّم عرضاً واحداً فقط (يمكن تحديثه بإعادة الاستدعاء)
        /// - الطلب يجب أن يكون مفتوحاً وضمن المهلة
        /// - السعر لا يتجاوز الميزانية القصوى (إن وُجدت)
        /// </summary>
        [HttpPost("{id:int}/offers")]
        [Authorize]
        [ProducesResponseType(typeof(ReverseAuctionOfferDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> SubmitOffer(int id, [FromBody] CreateReverseAuctionOfferDto dto)
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Unauthorized(new { message = "A verified company account is required to submit offers." });

            // تمرير الـ id من الـ route للـ DTO
            dto.ReverseAuctionId = id;

            var result = await _service.SubmitOfferAsync(companyId.Value, dto);
            if (!result.Succeeded)
                return BadRequest(new { message = result.Error });

            return CreatedAtAction(nameof(GetOffers), new { id }, result.Data);
        }

        /// <summary>
        /// عرض العروض المقدَّمة على طلب شراء.
        /// مرئي فقط لصاحب الطلب — الموردون الآخرون لا يرون عروض بعضهم.
        /// </summary>
        [HttpGet("{id:int}/offers")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<ReverseAuctionOfferDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetOffers(int id)
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Unauthorized(new { message = "A verified company account is required." });

            var offers = await _service.GetOffersAsync(id, companyId.Value);
            return Ok(offers);
        }

        /// <summary>
        /// قبول عرض وترسية الطلب على المورّد المختار.
        ///
        /// يُنشئ أمر شراء ويُبلّغ جميع الموردين بالنتيجة.
        /// </summary>
        [HttpPost("{id:int}/offers/{offerId:int}/award")]
        [Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> AwardOffer(int id, int offerId)
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Unauthorized(new { message = "A verified company account is required." });

            var result = await _service.AwardOfferAsync(id, offerId, companyId.Value);
            if (!result.Succeeded)
                return BadRequest(new { message = result.Error });

            return NoContent();
        }

        /// <summary>
        /// سحب عرض مقدَّم.
        /// فقط الشركة التي قدّمت العرض يمكنها سحبه، ولا يمكن سحب عرض تم قبوله.
        /// </summary>
        [HttpDelete("offers/{offerId:int}")]
        [Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> WithdrawOffer(int offerId)
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Unauthorized(new { message = "A verified company account is required." });

            var result = await _service.WithdrawOfferAsync(offerId, companyId.Value);
            if (!result.Succeeded)
                return BadRequest(new { message = result.Error });

            return NoContent();
        }

        /// <summary>
        /// العروض التي قدّمتها الشركة الحالية على طلبات شراء مختلفة (سجل المورّد).
        /// </summary>
        [HttpGet("my-offers")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<ReverseAuctionOfferDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetMyOffers()
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Unauthorized(new { message = "A verified company account is required." });

            var offers = await _service.GetMyOffersAsync(companyId.Value);
            return Ok(offers);
        }

        // ─── Helpers ───────────────────────────────────────────────────────────────

        private int? GetCurrentCompanyId()
        {
            var claim = User.FindFirst("companyId")?.Value;
            if (string.IsNullOrWhiteSpace(claim)) return null;
            return int.TryParse(claim, out var id) && id > 0 ? id : null;
        }

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst("uid")?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }
    }
}
