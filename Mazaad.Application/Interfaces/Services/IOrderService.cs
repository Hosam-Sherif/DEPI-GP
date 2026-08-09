using System.Collections.Generic;
using System.Threading.Tasks;
using Mazaad.Application.DTOs;

namespace Mazaad.Application.Interfaces.Services
{
    public interface IOrderService
    {
        /// <summary>List all orders for a company (as buyer or seller)</summary>
        Task<IEnumerable<OrderResponseDto>> GetOrdersForCompanyAsync(int companyId);

        Task<OrderResponseDto?> GetOrderByIdAsync(int orderId);

        /// <summary>Convert a winning bid into a formal order applying commission policy</summary>
        Task<OrderResponseDto> FinalizeOrderAsync(int sellerCompanyId, FinalizeOrderDto request);

        /// <summary>
        /// Persists the new status on the Order and enforces the escrow side-effects:
        ///
        /// Transition to Delivered:
        ///   After saving the new status, calls IEscrowService.ReleaseEscrowAsync(orderId).
        ///   If payout initiation fails (e.g., no verified bank account), the Delivered
        ///   status is still committed — delivered goods cannot be "un-delivered" —
        ///   but a Telegram alert fires to notify SuperAdmin of the failed payout.
        ///
        /// Transition to Cancelled:
        ///   If an EscrowRecord with Status = Held exists for this order,
        ///   calls IEscrowService.RefundEscrowAsync(orderId, reason) to return
        ///   funds to the buyer. If no escrow exists (order cancelled before payment),
        ///   the cancellation proceeds without a refund attempt.
        ///
        /// All other transitions:
        ///   Simple status update with no escrow side-effects.
        /// </summary>
        Task<bool> UpdateOrderStatusAsync(
            int orderId,
            int companyId,
            Domain.Enums.OrderStatus newStatus);
    }
}
