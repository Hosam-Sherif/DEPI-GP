using System.Collections.Generic;
using System.Threading.Tasks;
using Mazaad.Application.DTOs;

namespace Mazaad.Application.Interfaces
{
    public interface IListingService
    {
        /// <summary>Marketplace grid: filtered, paginated listing cards</summary>
        Task<PagedResultDto<ListingCardDto>> GetListingsAsync(ListingFilterDto filter);

        /// <summary>Single card summary (used internally and for backward compat)</summary>
        Task<ListingResponseDto?> GetListingByIdAsync(int id);

        /// <summary>Full bidding-room detail including top bids and specs</summary>
        Task<ListingDetailDto?> GetListingDetailAsync(int id);

        Task<ListingResponseDto> CreateListingAsync(int companyId, CreateListingDto request);

        /// <summary>Update a listing's mutable fields</summary>
        Task<ListingResponseDto?> UpdateListingAsync(int id, int companyId, CreateListingDto request);

        Task<bool> DeleteListingAsync(int id, int companyId);
    }
}