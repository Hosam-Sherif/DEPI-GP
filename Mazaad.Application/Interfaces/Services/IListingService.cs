using Mazaad.Application.Common;
using Mazaad.Application.DTOs;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mazaad.Application.Interfaces.Services
{
    public interface IListingService
    {
        /// <summary>Marketplace grid: filtered, paginated listing cards</summary>
        Task<PagedResultDto<ListingCardDto>> GetListingsAsync(ListingFilterDto filter);

        /// <summary>Dashboard "My Listings": filtered, paginated listing cards owned by the given company</summary>
        Task<PagedResultDto<ListingCardDto>> GetMyListingsAsync(int companyId, ListingFilterDto filter);

        /// <summary>Single card summary (used internally and for backward compat)</summary>
        Task<ListingResponseDto?> GetListingByIdAsync(int id);

        /// <summary>Full bidding-room detail including top bids and specs</summary>
        Task<ListingDetailDto?> GetListingDetailAsync(int id);

        Task<ListingResponseDto> CreateListingAsync(int companyId, CreateListingDto request);

        /// <summary>Update a listing's mutable fields</summary>
        Task<ListingResponseDto?> UpdateListingAsync(int id, int companyId, CreateListingDto request);

        Task<ListingResponseDto?> UploadListingImageAsync(int listingId, int companyId, IFormFile image);

        Task<bool> DeleteListingAsync(int id, int companyId);

        /// <summary>Cancel a listing (sets status to Cancelled without deleting)</summary>
        Task<bool> CancelListingAsync(int id, int companyId);

        /// <summary>
        /// Ends an Active/Upcoming auction immediately (sets status to Ended, EndDate = now)
        /// and returns the outcome, including the winning bid if one exists.
        /// Returns null if the listing doesn't exist, isn't owned by the company,
        /// or is already Ended/Cancelled.
        /// </summary>
        Task<EndAuctionResultDto?> EndListingNowAsync(int id, int companyId);

        /// <summary>SuperAdmin queue: listings currently awaiting approval, oldest first.</summary>
        Task<IEnumerable<PendingListingDto>> GetPendingListingsAsync();

        /// <summary>
        /// SuperAdmin approves or rejects a PendingApproval listing.
        /// Approving moves it to Upcoming/Active (based on StartDate); rejecting moves it to Rejected.
        /// </summary>
        Task<Result> ApproveListingAsync(int listingId, int adminUserId, ApproveListingDto dto, string ipAddress);
    }
}