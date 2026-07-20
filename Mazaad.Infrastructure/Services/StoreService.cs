using Mazaad.Application.Common;
using Mazaad.Application.DTOs.Store;
using Mazaad.Application.Interfaces.Services;
using Mazaad.Domain.Models;
using Mazaad.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Mazaad.Infrastructure.Services
{
    public class StoreService : IStoreService
    {
        private readonly AppDbContext _context;
        private readonly IImageService _imageService;

        public StoreService(AppDbContext context, IImageService imageService)
        {
            _context = context;
            _imageService = imageService;
        }

        public async Task<Result<StoreResponseDto>> CreateStoreAsync(int companyId, CreateStoreDto dto, IFormFile? logo)
        {
            var company = await _context.Companies.FindAsync(companyId);
            if (company == null)
                return Result<StoreResponseDto>.Failure("Company not found");

            var exists = await _context.Stores.AnyAsync(s => s.CompanyId == companyId);
            if (exists)
                return Result<StoreResponseDto>.Failure("Company already has a store");

            var slugTaken = await _context.Stores.AnyAsync(s => s.Slug == dto.Slug.ToLower());
            if (slugTaken)
                return Result<StoreResponseDto>.Failure("This store URL is already taken");

            string? logoUrl = null;
            if (logo != null)
                logoUrl = await _imageService.UploadImageAsync(
                    logo.OpenReadStream(),
                    logo.FileName,
                    "store-logos"
                );

            var store = new Store
            {
                CompanyId = companyId,
                Name = dto.Name,
                Slug = dto.Slug.ToLower().Trim(),
                Description = dto.Description,
                Color = dto.Color,
                LogoUrl = logoUrl,
            };

            _context.Stores.Add(store);
            await _context.SaveChangesAsync();

            return Result<StoreResponseDto>.Success(MapToDto(store, company.CompanyName));
        }

        public async Task<Result<StoreResponseDto>> GetMyStoreAsync(int companyId)
        {
            var store = await _context.Stores
                .Include(s => s.Company)
                .FirstOrDefaultAsync(s => s.CompanyId == companyId);

            if (store == null)
                return Result<StoreResponseDto>.Failure("No store found");

            return Result<StoreResponseDto>.Success(MapToDto(store, store.Company.CompanyName));
        }

        public async Task<Result<StoreResponseDto>> GetStoreBySlugAsync(string slug)
        {
            var store = await _context.Stores
                .Include(s => s.Company)
                .FirstOrDefaultAsync(s => s.Slug == slug.ToLower() && s.IsActive);

            if (store == null)
                return Result<StoreResponseDto>.Failure("Store not found");

            return Result<StoreResponseDto>.Success(MapToDto(store, store.Company.CompanyName));
        }

        public async Task<Result<StoreResponseDto>> UpdateStoreAsync(int companyId, UpdateStoreDto dto, IFormFile? logo)
        {
            var store = await _context.Stores
                .Include(s => s.Company)
                .FirstOrDefaultAsync(s => s.CompanyId == companyId);

            if (store == null)
                return Result<StoreResponseDto>.Failure("Store not found");

            if (dto.Slug != null && dto.Slug.ToLower() != store.Slug)
            {
                var slugTaken = await _context.Stores
                    .AnyAsync(s => s.Slug == dto.Slug.ToLower() && s.CompanyId != companyId);
                if (slugTaken)
                    return Result<StoreResponseDto>.Failure("This store URL is already taken");

                store.Slug = dto.Slug.ToLower().Trim();
            }

            if (dto.Name != null) store.Name = dto.Name;
            if (dto.Description != null) store.Description = dto.Description;
            if (dto.Color != null) store.Color = dto.Color;

            if (logo != null)
                store.LogoUrl = await _imageService.UploadImageAsync(
                    logo.OpenReadStream(),
                    logo.FileName,
                    "store-logos"
                );

            store.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Result<StoreResponseDto>.Success(MapToDto(store, store.Company.CompanyName));
        }

        public async Task<Result<bool>> IsSlugAvailableAsync(string slug)
        {
            var taken = await _context.Stores.AnyAsync(s => s.Slug == slug.ToLower());
            return Result<bool>.Success(!taken);
        }

        private static StoreResponseDto MapToDto(Store store, string companyName) => new()
        {
            Id = store.Id,
            CompanyId = store.CompanyId,
            CompanyName = companyName,
            Name = store.Name,
            Slug = store.Slug,
            StoreUrl = $"https://mazzad-front-end.vercel.app/store/{store.Slug}",
            Description = store.Description,
            LogoUrl = store.LogoUrl,
            Color = store.Color,
            IsActive = store.IsActive,
            CreatedAt = store.CreatedAt
        };
    }
}