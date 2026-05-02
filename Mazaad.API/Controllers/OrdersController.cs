using System.Threading.Tasks;
using Mazaad.Application.DTOs;
using Mazaad.Application.Interfaces;
using Mazaad.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Mazaad.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>List all orders where the current company is buyer or seller.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // TODO: extract from JWT
            int currentCompanyId = 1;
            var orders = await _orderService.GetOrdersForCompanyAsync(currentCompanyId);
            return Ok(orders);
        }

        /// <summary>Get a single order by ID.</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null) return NotFound();
            return Ok(order);
        }

        /// <summary>
        /// Finalize a winning bid into a formal order.
        /// Applies the active commission policy automatically.
        /// </summary>
        [HttpPost("finalize")]
        public async Task<IActionResult> Finalize([FromBody] FinalizeOrderDto request)
        {
            int currentCompanyId = 1;
            try
            {
                var order = await _orderService.FinalizeOrderAsync(currentCompanyId, request);
                return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (System.UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        /// <summary>Update the status of an order (Pending → Confirmed → Completed).</summary>
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] OrderStatus newStatus)
        {
            int currentCompanyId = 1;
            var success = await _orderService.UpdateOrderStatusAsync(id, currentCompanyId, newStatus);
            if (!success) return BadRequest("Could not update order status.");
            return NoContent();
        }
    }
}
