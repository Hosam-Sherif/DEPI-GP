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
    }
}