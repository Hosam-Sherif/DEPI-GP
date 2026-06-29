using System.Text.Json;
using System.Threading.Tasks;
using Mazaad.Application.DTOs;
using Mazaad.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mazaad.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        private int? GetCurrentCompanyId()
        {
            var claim = User.FindFirst("companyId")?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }

        /// <summary>Starts a Paymob payment for an order and returns the iframe URL to open.</summary>
        [HttpPost("initiate")]
        [Authorize]
        public async Task<IActionResult> Initiate([FromBody] CreatePaymentRequestDto request)
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null) return Forbid();

            try
            {
                var result = await _paymentService.InitiatePaymentAsync(companyId.Value, request);
                return Ok(result);
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

        /// <summary>Get the latest payment status for a given order (for the frontend to poll).</summary>
        [HttpGet("order/{orderId}")]
        [Authorize]
        public async Task<IActionResult> GetByOrder(int orderId)
        {
            var payment = await _paymentService.GetPaymentForOrderAsync(orderId);
            if (payment == null) return NotFound();
            return Ok(payment);
        }

        /// <summary>
        /// Paymob calls this endpoint server-to-server after the transaction is processed.
        /// Must stay anonymous (Paymob has no JWT) — security comes from the HMAC check instead.
        /// </summary>
        [HttpPost("paymob/webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> PaymobWebhook([FromBody] JsonElement payload)
        {
            var hmac = Request.Query["hmac"].ToString();
            var accepted = await _paymentService.HandlePaymobWebhookAsync(payload, hmac);

            // Always 200 so Paymob doesn't keep retrying; rejection is logged internally via 'accepted'.
            return Ok(new { received = accepted });
        }
    }
}