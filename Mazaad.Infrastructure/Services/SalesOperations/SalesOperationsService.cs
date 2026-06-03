using Mazaad.Application.DTOs.Sales;
using Mazaad.Application.Interfaces.Repositories;
using Mazaad.Application.Interfaces.Services;
using System.Threading.Tasks;

namespace Mazaad.Infrastructure.Services.SalesOperations
{
    public class SalesOperationsService
        : ISalesOperationsService
    {
        private readonly ISalesRepository _repository;

        public SalesOperationsService(
            ISalesRepository repository)
        {
            _repository = repository;
        }

        public async Task<DashboardStatisticsDto>
            GetDashboardAsync(int companyId)
        {
            return new DashboardStatisticsDto
            {
                TotalRevenue =
                    await _repository
                        .GetTotalRevenueAsync(companyId),

                TotalOrders =
                    await _repository
                        .GetOrdersCountAsync(companyId),

                ActiveAuctions =
                    await _repository
                        .GetActiveAuctionsCountAsync(companyId),

                InventoryCount =
                    await _repository
                        .GetInventoryCountAsync(companyId)
            };
        }
    }
}