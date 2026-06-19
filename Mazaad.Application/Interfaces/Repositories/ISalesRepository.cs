using System.Threading.Tasks;

namespace Mazaad.Application.Interfaces.Repositories
{
    public interface ISalesRepository
    {
        Task<decimal> GetTotalRevenueAsync(int companyId);
        Task<int> GetOrdersCountAsync(int companyId);
        Task<int> GetActiveAuctionsCountAsync(int companyId);
        Task<int> GetInventoryCountAsync(int companyId);
    }
}
