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
    public class BiddingService : IBiddingService
    {
        private readonly AppDbContext _context;

        public BiddingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<BidResultDto> PlaceBidAsync(int userId, int companyId, PlaceBidDto request)
        {
            var listing = await _context.Listings.FindAsync(request.ListingId);

            if (listing == null)
                return new BidResultDto { Success = false, Message = "Listing not found." };

            if (listing.EndDate <= DateTime.UtcNow)
                return new BidResultDto { Success = false, Message = "Sorry, the auction has ended." };

            if (request.Quantity < listing.MinOrderQuantity)
                return new BidResultDto { Success = false, Message = $"You must bid on at least {listing.MinOrderQuantity} units." };

            if (request.Quantity > listing.AvailableQuantity)
                return new BidResultDto { Success = false, Message = $"Only {listing.AvailableQuantity} units are available." };

            // calculate the total
            decimal expectedTotal = request.BidAmountPerUnit * request.Quantity;
            if (request.BidAmountPerUnit != expectedTotal)
                return new BidResultDto { Success = false, Message = "Data mismatch. The total amount does not match the quantity * unit price." };

            if (request.BidAmountPerUnit <= listing.CurrentHighestBid)
                return new BidResultDto { Success = false, Message = "The bid amount must be higher than the current price." };

            listing.CurrentHighestBid = request.BidAmountPerUnit;

            var bid = new Bids
            {
                ListingId = request.ListingId,
                BuyerCompanyId = companyId,
                PlacedByUserId = userId,
                BidAmountPerUnit = request.BidAmountPerUnit,
                TotalBidAmount = request.BidAmountPerUnit,
                Quantity = request.Quantity,
                IsAnonymous = request.IsAnonymous,
                CreatedAt = DateTime.UtcNow
            };

            _context.Bids.Add(bid);

            try
            {
                await _context.SaveChangesAsync();
                Companies company = await _context.Companies.FindAsync(companyId);
                string displayBider = request.IsAnonymous ? "Anonymous" : $"Company {company.CompanyName}";

                return new BidResultDto
                {
                    Success = true,
                    Message = "Bid placed successfully",
                    DisplayBiddersName = displayBider,
                    NewPrice = request.BidAmountPerUnit
                };
            }
            catch (DbUpdateConcurrencyException)
            {
                return new BidResultDto
                {
                    Success = false,
                    Message = "Sorry, the bid could not be placed due to a concurrency issue. Please try again."
                };
            }
        }

        public async Task<IEnumerable<BidResultDto>> GetBidsForListingAsync(int listingId)
        {
            var bids = await _context.Bids.Include(b => b.BuyerCompany)
                .Where(b => b.ListingId == listingId)
                .OrderByDescending(b => b.BidAmountPerUnit)
                .ToListAsync();

            return bids.Select(b => new BidResultDto
            {
                Success = true,
                Message = "Bid retrieved",
                DisplayBiddersName = b.IsAnonymous ? "Anonymous" : $"Company {b.BuyerCompany.CompanyName}",
                NewPrice = b.BidAmountPerUnit
            });
        }

        public async Task<bool> DeleteBidAsync(int bidId, int companyId)
        {
            var bid = await _context.Bids.FindAsync(bidId);

            if (bid == null || bid.BuyerCompanyId != companyId)
                return false;

            _context.Bids.Remove(bid);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
