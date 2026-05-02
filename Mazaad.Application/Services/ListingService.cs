using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mazaad.Application.DTOs;
using Mazaad.Application.Interfaces;
using Mazaad.Domain.Enums;
using Mazaad.Domain.Models;
using Mazaad.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mazaad.Application.Services
{
    public class ListingService : IListingService
    {
        private readonly AppDbContext _context;

        public ListingService(AppDbContext context)
        {
            _context = context;
        }

        // ─── Marketplace Grid ──────────────────────────────────────────────────────

        public async Task<PagedResultDto<ListingCardDto>> GetListingsAsync(ListingFilterDto filter)
        {
            var query = _context.Listings
                .Include(l => l.Category)
                .Include(l => l.Company)
                .Where(l => !l.IsDeleted);

            // Apply filters
            if (filter.CategoryId.HasValue)
                query = query.Where(l => l.CategoryId == filter.CategoryId.Value);

            if (filter.Condition.HasValue)
                query = query.Where(l => l.Condition == filter.Condition.Value);

            if (filter.Status.HasValue)
                query = query.Where(l => l.Status == filter.Status.Value);

            if (filter.MinPrice.HasValue)
                query = query.Where(l => l.CurrentHighestBid >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                query = query.Where(l => l.CurrentHighestBid <= filter.MaxPrice.Value);

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLower();
                query = query.Where(l =>
                    l.Title.ToLower().Contains(term) ||
                    l.Description.ToLower().Contains(term));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(l => new ListingCardDto
                {
                    Id = l.Id,
                    Title = l.Title,
                    Description = l.Description,
                    ImageUrl = l.ImageUrl,
                    CategoryName = l.Category.CategoryName,
                    CompanyName = l.Company.CompanyName,
                    CurrentHighestBid = l.CurrentHighestBid,
                    BidCount = l.BidCount,
                    Status = l.Status,
                    Condition = l.Condition,
                    BaseCurrency = l.BaseCurrency,
                    EndDate = l.EndDate
                })
                .ToListAsync();

            return new PagedResultDto<ListingCardDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        // ─── Single Listing Summary ────────────────────────────────────────────────

        public async Task<ListingResponseDto?> GetListingByIdAsync(int id)
        {
            var listing = await _context.Listings.FindAsync(id);
            if (listing == null || listing.IsDeleted) return null;

            return MapToResponseDto(listing);
        }

        // ─── Full Bidding-Room Detail ──────────────────────────────────────────────

        public async Task<ListingDetailDto?> GetListingDetailAsync(int id)
        {
            var listing = await _context.Listings
                .Include(l => l.Category)
                .Include(l => l.Company)
                .Include(l => l.Bids.OrderByDescending(b => b.BidAmountPerUnit).Take(5))
                    .ThenInclude(b => b.BuyerCompany)
                .FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);

            if (listing == null) return null;

            return new ListingDetailDto
            {
                Id = listing.Id,
                CompanyId = listing.CompanyId,
                CompanyName = listing.Company.CompanyName,
                CategoryId = listing.CategoryId,
                CategoryName = listing.Category.CategoryName,
                Title = listing.Title,
                Description = listing.Description,
                TechnicalSpecs = listing.TechnicalSpecs,
                MinOrderQuantity = listing.MinOrderQuantity,
                AvailableQuantity = listing.AvailableQuantity,
                PurityPercentage = listing.PurityPercentage,
                BaseCurrency = listing.BaseCurrency,
                CurrentHighestBid = listing.CurrentHighestBid,
                BidCount = listing.BidCount,
                Status = listing.Status,
                Condition = listing.Condition,
                ImageUrl = listing.ImageUrl,
                Location = listing.Location,
                DueDiligenceUrls = listing.DueDiligenceUrls,
                StartDate = listing.StartDate,
                EndDate = listing.EndDate,
                TopBids = listing.Bids.Select(b => new BidDetailDto
                {
                    Id = b.Id,
                    ListingId = b.ListingId,
                    BuyerCompanyId = b.BuyerCompanyId,
                    DisplayBidderName = b.IsAnonymous ? "Anonymous" : b.BuyerCompany.CompanyName,
                    BidAmountPerUnit = b.BidAmountPerUnit,
                    TotalBidAmount = b.TotalBidAmount,
                    Quantity = b.Quantity,
                    IsAnonymous = b.IsAnonymous,
                    Status = b.Status,
                    CreatedAt = b.CreatedAt
                })
            };
        }

        // ─── Create ───────────────────────────────────────────────────────────────

        public async Task<ListingResponseDto> CreateListingAsync(int companyId, CreateListingDto request)
        {
            var listing = new Listings
            {
                CompanyId = companyId,
                CategoryId = request.CategoryId,
                Title = request.Title,
                Description = request.Description,
                MinOrderQuantity = request.MinOrderQuantity,
                AvailableQuantity = request.AvailableQuantity,
                PurityPercentage = request.PurityPercentage,
                BaseCurrency = request.BaseCurrency,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                CurrentHighestBid = request.StartingPrice,
                Status = request.StartDate > DateTime.UtcNow ? ListingStatus.Upcoming : ListingStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Listings.Add(listing);
            await _context.SaveChangesAsync();

            return MapToResponseDto(listing);
        }

        // ─── Update ───────────────────────────────────────────────────────────────

        public async Task<ListingResponseDto?> UpdateListingAsync(int id, int companyId, CreateListingDto request)
        {
            var listing = await _context.Listings.FindAsync(id);

            if (listing == null || listing.IsDeleted || listing.CompanyId != companyId)
                return null;

            listing.CategoryId = request.CategoryId;
            listing.Title = request.Title;
            listing.Description = request.Description;
            listing.MinOrderQuantity = request.MinOrderQuantity;
            listing.AvailableQuantity = request.AvailableQuantity;
            listing.PurityPercentage = request.PurityPercentage;
            listing.BaseCurrency = request.BaseCurrency;
            listing.StartDate = request.StartDate;
            listing.EndDate = request.EndDate;
            listing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToResponseDto(listing);
        }

        // ─── Soft Delete ──────────────────────────────────────────────────────────

        public async Task<bool> DeleteListingAsync(int id, int companyId)
        {
            var listing = await _context.Listings.FindAsync(id);

            if (listing == null || listing.CompanyId != companyId)
                return false;

            listing.IsDeleted = true;
            listing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ─── Private Helpers ──────────────────────────────────────────────────────

        private static ListingResponseDto MapToResponseDto(Listings listing) => new ListingResponseDto
        {
            Id = listing.Id,
            CompanyId = listing.CompanyId,
            CategoryId = listing.CategoryId,
            Title = listing.Title,
            Description = listing.Description,
            MinOrderQuantity = listing.MinOrderQuantity,
            AvailableQuantity = listing.AvailableQuantity,
            PurityPercentage = listing.PurityPercentage,
            BaseCurrency = listing.BaseCurrency,
            StartDate = listing.StartDate,
            EndDate = listing.EndDate,
            CurrentHighestBid = listing.CurrentHighestBid
        };
    }
}
