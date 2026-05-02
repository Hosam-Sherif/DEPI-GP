using System.Collections.Generic;
using System.Threading.Tasks;
using Mazaad.Application.DTOs;

namespace Mazaad.Application.Interfaces
{
    public interface IOrderService
    {
        /// <summary>List all orders for a company (as buyer or seller)</summary>
        Task<IEnumerable<OrderResponseDto>> GetOrdersForCompanyAsync(int companyId);

        Task<OrderResponseDto?> GetOrderByIdAsync(int orderId);

        /// <summary>Convert a winning bid into a formal order applying commission policy</summary>
        Task<OrderResponseDto> FinalizeOrderAsync(int sellerCompanyId, FinalizeOrderDto request);

        Task<bool> UpdateOrderStatusAsync(int orderId, int companyId, Domain.Enums.OrderStatus newStatus);
    }
}
