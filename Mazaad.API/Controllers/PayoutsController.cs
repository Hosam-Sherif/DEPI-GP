using System;
using System.Text.Json;
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
    public class PayoutsController : ControllerBase
    {
        private readonly IPayoutService _payoutService;

        public PayoutsController(IPayoutService payoutService)
        {
            _payoutService = payoutService;
        }

        private int? GetCurrentCompanyId()
        {
            var claim = User.FindFirst("companyId")?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }

        /// <summary>
        /// Retrieves the payout history for the logged-in seller company.
        /// </summary>
        [HttpGet("my-payouts")]
        public async Task<IActionResult> GetMyPayouts()
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Forbid("Access denied: missing company context.");

            var payouts = await _payoutService.GetPayoutsForSellerAsync(companyId.Value);
            return Ok(payouts);
        }

        /// <summary>
        /// Retrieves the detail of a specific payout attempt.
        /// Accessible by the seller company of the payout or any SuperAdmin.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var isSuperAdmin = User.IsInRole("SuperAdmin");
            var companyId = GetCurrentCompanyId();

            if (!isSuperAdmin && companyId == null)
                return Forbid("Access denied: missing authorization context.");

            var payout = await _payoutService.GetPayoutByIdAsync(id);
            if (payout == null)
                return NotFound(new { message = "Payout record not found." });

            if (!isSuperAdmin && payout.SellerCompanyId != companyId!.Value)
                return Forbid("Access denied: this payout belongs to another company.");

            return Ok(payout);
        }

        /// <summary>
        /// Lists all payout records held by the platform.
        /// SuperAdmin only.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetAll([FromQuery] PayoutStatus? status)
        {
            var result = await _payoutService.GetAllPayoutsAsync(status);
            return Ok(result);
        }

        /// <summary>
        /// Manually re-initiates a failed disbursement to the seller.
        /// SuperAdmin only.
        /// </summary>
        [HttpPost("{id}/retry")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Retry(int id)
        {
            try
            {
                var result = await _payoutService.RetryPayoutAsync(id);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Paymob calls this server-to-server webhook after a disbursement transfer is processed.
        /// HMAC is checked internally inside the payout service logic.
        /// </summary>
        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> PayoutWebhook([FromBody] JsonElement payload)
        {
            // Read HMAC from header or query parameters for maximum integration flexibility
            var signature = Request.Headers["X-SHA512-Signature"].ToString();
            if (string.IsNullOrEmpty(signature))
            {
                signature = Request.Query["hmac"].ToString();
            }

            if (string.IsNullOrEmpty(signature))
            {
                return Unauthorized(new { message = "Missing signature/HMAC verification parameters." });
            }

            var accepted = await _payoutService.HandlePayoutWebhookAsync(payload, signature);

            // Respond 200 OK so Paymob stops retry schedules
            return Ok(new { received = accepted });
        }
    }
}
