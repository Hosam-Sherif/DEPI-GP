using System.Threading.Tasks;
using Mazaad.Domain.Models;

namespace Mazaad.Application.Interfaces.Repositories
{
    public interface IBiddingRepository
    {
        Task<Listings?> GetListingAsync(int listingId);
        Task AddBidAsync(Bids bid);
        Task SaveChangesAsync();
    }
}
