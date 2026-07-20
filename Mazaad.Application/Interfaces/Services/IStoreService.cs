using Mazaad.Application.Common;
using Mazaad.Application.DTOs.Store;
using Microsoft.AspNetCore.Http;

namespace Mazaad.Application.Interfaces.Services
{
    public interface IStoreService
    {
        // Company creates their store after registration
        Task<Result<StoreResponseDto>> CreateStoreAsync(int companyId, CreateStoreDto dto, IFormFile? logo);

        // Get my store (CompanyAdmin)
        Task<Result<StoreResponseDto>> GetMyStoreAsync(int companyId);

        // Public store page — by slug
        Task<Result<StoreResponseDto>> GetStoreBySlugAsync(string slug);

        // Update store info
        Task<Result<StoreResponseDto>> UpdateStoreAsync(int companyId, UpdateStoreDto dto, IFormFile? logo);

        // Check if slug is available
        Task<Result<bool>> IsSlugAvailableAsync(string slug);
    }
}