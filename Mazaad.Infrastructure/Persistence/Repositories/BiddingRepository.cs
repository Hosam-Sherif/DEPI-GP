using Mazaad.Domain.Models;
using Mazaad.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Mazaad.Infrastructure.Persistence.Repositories
{
    public class BiddingRepository : IBiddingRepository
    {
        private readonly AppDbContext _context;

        public BiddingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Listings?> GetListingAsync(int listingId)
        {
            return await _context.Listings
                .Include(x => x.Bids)
                .Include(x => x.Company)
                .FirstOrDefaultAsync(x => x.ID == listingId);
        }

        public async Task AddBidAsync(Bids bid)
        {
            await _context.Bids.AddAsync(bid);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}