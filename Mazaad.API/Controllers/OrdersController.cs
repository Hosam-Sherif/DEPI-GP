using System.Threading.Tasks;
using Mazaad.Application.DTOs;
using Mazaad.Application.Interfaces.Services;
using Mazaad.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mazaad.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private int? GetCurrentCompanyId()
        {
            var claim = User.FindFirst("companyId")?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }

        // ── Endpoints ─────────────────────────────────────────────────────────────

        /// <summary>List all orders where the current company is buyer or seller.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null) return Forbid();

            var orders = await _orderService.GetOrdersForCompanyAsync(companyId.Value);
            return Ok(orders);
        }

        /// <summary>Get a single order by ID.</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null) return Forbid();

            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null) return NotFound();

            // ownership check — الشركة لازم تكون buyer أو seller
            if (order.SellerCompanyId != companyId.Value && order.BuyerCompanyId != companyId.Value)
                return Forbid();

            return Ok(order);
        }

        /// <summary>
        /// Finalize a winning bid into a formal order.
        /// Applies the active commission policy automatically.
        /// </summary>
        [HttpPost("finalize")]
        [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
        public async Task<IActionResult> Finalize([FromBody] FinalizeOrderDto request)
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null) return Forbid();

            try
            {
                var order = await _orderService.FinalizeOrderAsync(companyId.Value, request);
                return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (System.UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        /// <summary>Update the status of an order (Pending → Confirmed → Completed).</summary>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] OrderStatus newStatus)
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null) return Forbid();

            var success = await _orderService.UpdateOrderStatusAsync(id, companyId.Value, newStatus);
            if (!success) return BadRequest(new { message = "Could not update order status." });

            return NoContent();
        }
    }
}