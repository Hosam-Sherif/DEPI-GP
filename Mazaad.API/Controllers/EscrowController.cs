using System;
using System.Threading.Tasks;
using Mazaad.Application.Interfaces.Services;
using Mazaad.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mazaad.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EscrowController : ControllerBase
    {
        private readonly IEscrowService _escrowService;
        private readonly IOrderService _orderService;

        public EscrowController(IEscrowService escrowService, IOrderService orderService)
        {
            _escrowService = escrowService;
            _orderService = orderService;
        }

        private int? GetCurrentCompanyId()
        {
            var claim = User.FindFirst("companyId")?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }

        /// <summary>
        /// Retrieves the escrow record details for a specific order.
        /// Accessible by the buyer company, the seller company, or any SuperAdmin.
        /// </summary>
        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetByOrder(int orderId)
        {
            var isSuperAdmin = User.IsInRole("SuperAdmin");
            var companyId = GetCurrentCompanyId();

            if (!isSuperAdmin && companyId == null)
                return Forbid("Access denied: missing company authorization.");

            // Verify order belongs to the caller's company if they are not a SuperAdmin
            if (!isSuperAdmin)
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order == null)
                    return NotFound(new { message = "Order not found." });

                if (order.BuyerCompanyId != companyId!.Value && order.SellerCompanyId != companyId.Value)
                    return Forbid("Access denied: you are not a participant in this order.");
            }

            var escrow = await _escrowService.GetEscrowForOrderAsync(orderId);
            if (escrow == null)
                return NotFound(new { message = "No escrow custody record found for this order. Payment may still be pending." });

            return Ok(escrow);
        }

        /// <summary>
        /// Lists all escrow records held by the platform.
        /// SuperAdmin only.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetAll([FromQuery] EscrowStatus? status)
        {
            var result = await _escrowService.GetAllEscrowsAsync(status);
            return Ok(result);
        }
    }
}
