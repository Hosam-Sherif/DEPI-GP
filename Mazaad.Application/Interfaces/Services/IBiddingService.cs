using System.Collections.Generic;
using System.Threading.Tasks;
using Mazaad.Application.DTOs;

namespace Mazaad.Application.Interfaces.Services
{
    public interface IBiddingService
    {
        /// <summary>Full bid from the bidding room execution panel. companyId is null for individual (non-company) bidders.</summary>
        Task<BidResultDto> PlaceBidAsync(int userId, int? companyId, PlaceBidDto request);   // 🔴 تعديل: int companyId → int? companyId

        /// <summary>One-click quick bid from a marketplace card (uses MinOrderQuantity). companyId is null for individual (non-company) bidders.</summary>
        Task<BidResultDto> PlaceQuickBidAsync(int userId, int? companyId, QuickBidDto request);   // 🔴 تعديل: نفس الموضوع

        Task<IEnumerable<BidResultDto>> GetBidsForListingAsync(int listingId);
        Task<IEnumerable<BidDetailDto>> GetLiveBidsAsync(int listingId);
        Task<BidDetailDto?> GetBidDetailAsync(int bidId);

        Task<IEnumerable<BidDetailDto>> GetBidsByCompanyAsync(int companyId);   // زي ما هي، سبناها لأي حد لسه محتاجها

        /// <summary>All bids placed by a specific user, regardless of company (for My Bids page — company users and individual bidders alike)</summary>
        Task<IEnumerable<BidDetailDto>> GetBidsByUserAsync(int userId);   // 🔴 تعديل: Method جديدة بالكامل

        /// <summary>Cancel a bid. Ownership is verified by the placing user, since individual bidders have no companyId.</summary>
        Task<bool> DeleteBidAsync(int bidId, int userId);   // 🔴 تعديل: كانت (int bidId, int companyId)
    }
}