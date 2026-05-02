using System.Collections.Generic;
using System.Threading.Tasks;
using Mazaad.Application.DTOs;

namespace Mazaad.Application.Interfaces
{
    public interface IListingService
    {
        Task<IEnumerable<ListingResponseDto>> GetAllListingsAsync();
        Task<ListingResponseDto> GetListingByIdAsync(int id);
        Task<ListingResponseDto> CreateListingAsync(int companyId, CreateListingDto request);
        Task<bool> DeleteListingAsync(int id, int companyId);
    }
}