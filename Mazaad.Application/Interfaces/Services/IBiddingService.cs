using System.Collections.Generic;
using System.Threading.Tasks;
using Mazaad.Application.DTOs;

namespace Mazaad.Application.Interfaces.Services
{
    public interface IBiddingService
    {
        /// <summary>Full bid from the bidding room execution panel</summary>
        Task<BidResultDto> PlaceBidAsync(int userId, int companyId, PlaceBidDto request);

        /// <summary>One-click quick bid from a marketplace card (uses MinOrderQuantity)</summary>
        Task<BidResultDto> PlaceQuickBidAsync(int userId, int companyId, QuickBidDto request);

        /// <summary>All bids for a listing ordered by amount desc (basic result)</summary>
        Task<IEnumerable<BidResultDto>> GetBidsForListingAsync(int listingId);

        /// <summary>Live state: current highest bid + top bids with full detail (for SignalR init)</summary>
        Task<IEnumerable<BidDetailDto>> GetLiveBidsAsync(int listingId);

        /// <summary>Single bid with full detail</summary>
        Task<BidDetailDto?> GetBidDetailAsync(int bidId);

        /// <summary>All bids placed by a specific company (for My Bids page)</summary>
        Task<IEnumerable<BidDetailDto>> GetBidsByCompanyAsync(int companyId);

        /// <summary>Cancel a bid</summary>
        Task<bool> DeleteBidAsync(int bidId, int companyId);
    }
}