using System.Threading.Tasks;
using Mazaad.Application.DTOs;

namespace Mazaad.Application.Interfaces
{
    public interface IBiddingService
    {
        Task<BidResultDto> PlaceBidAsync(int userId, int companyId, PlaceBidDto request);
        Task<IEnumerable<BidResultDto>> GetBidsForListingAsync(int listingId);
        Task<bool> DeleteBidAsync(int bidId, int companyId);
    }
}