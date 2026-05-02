using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mazaad.Application.DTOs;
using Mazaad.Application.Interfaces;
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

        public async Task<IEnumerable<ListingResponseDto>> GetAllListingsAsync()
        {
            var listings = await _context.Listings
                .Where(l => !l.IsDeleted)
                .Select(l => new ListingResponseDto
                {
                    Id = l.Id,
                    CompanyId = l.CompanyId,
                    CategoryId = l.CategoryId,
                    Title = l.Title,
                    Description = l.Description,
                    MinOrderQuantity = l.MinOrderQuantity,
                    AvailableQuantity = l.AvailableQuantity,
                    PurityPercentage = l.PurityPercentage,
                    BaseCurrency = l.BaseCurrency,
                    StartDate = l.StartDate,
                    EndDate = l.EndDate,
                    CurrentHighestBid = l.CurrentHighestBid
                })
                .ToListAsync();

            return listings;
        }

        public async Task<ListingResponseDto?> GetListingByIdAsync(int id)
        {
            var listing = await _context.Listings.FindAsync(id);
            if (listing == null || listing.IsDeleted) return null;

            return new ListingResponseDto
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
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Listings.Add(listing);
            await _context.SaveChangesAsync();

            return new ListingResponseDto
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
    }
}
