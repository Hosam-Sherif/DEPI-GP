using Mazaad.Application.Interfaces.Repositories;
using Mazaad.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Mazaad.Infrastructure.Persistence.Repositories
{
    public class SalesRepository : ISalesRepository
    {
        private readonly AppDbContext _context;

        public SalesRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<decimal>
            GetTotalRevenueAsync(int companyId)
        {
            return await _context.Bids
                .Where(x => x.Listing.CompanyId == companyId)
                .SumAsync(x => x.TotalBidAmount);
        }

        public async Task<int>
            GetOrdersCountAsync(int companyId)
        {
            return await _context.Orders
                .CountAsync(x =>
                    x.SellerCompanyId == companyId ||
                    x.BuyerCompanyId == companyId);
        }

        public async Task<int>
            GetActiveAuctionsCountAsync(int companyId)
        {
            return await _context.Listings
                .CountAsync(x =>
                    x.CompanyId == companyId &&
                    x.EndDate > DateTime.UtcNow);
        }

        public async Task<int>
            GetInventoryCountAsync(int companyId)
        {
            return await _context.Listings
                .CountAsync(x => x.CompanyId == companyId);
        }
    }
}
