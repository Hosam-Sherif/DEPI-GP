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
    public class BiddingService : IBiddingService
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public BiddingService(
            AppDbContext context,
            INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // ─── Place Full Bid ───────────────────────────────────────────────────────

        public async Task<BidResultDto> PlaceBidAsync(int userId, int companyId, PlaceBidDto request)
        {
            var listing = await _context.Listings.FindAsync(request.ListingId);

            if (listing == null)
                return Fail("Listing not found.");

            if (listing.IsDeleted || listing.Status == ListingStatus.Cancelled)
                return Fail("This listing is no longer active.");

            if (listing.EndDate <= DateTime.UtcNow)
                return Fail("Sorry, the auction has ended.");

            if (request.Quantity < listing.MinOrderQuantity)
                return Fail($"You must bid on at least {listing.MinOrderQuantity} units.");

            if (request.Quantity > listing.AvailableQuantity)
                return Fail($"Only {listing.AvailableQuantity} units are available.");

            if (request.BidAmountPerUnit <= listing.CurrentHighestBid)
                return Fail("Your bid must exceed the current highest bid.");

            // Mark previous active bids for this listing as Outbid
            var previousBids = await _context.Bids
                .Where(b => b.ListingId == request.ListingId && b.Status == BidStatus.Active)
                .ToListAsync();
            foreach (var prev in previousBids)
                prev.Status = BidStatus.Outbid;

            // Update listing
            listing.CurrentHighestBid = request.BidAmountPerUnit;
            listing.BidCount++;
            if (listing.Status == ListingStatus.Upcoming)
                listing.Status = ListingStatus.Active;

            var bid = new Bids
            {
                ListingId = request.ListingId,
                BuyerCompanyId = companyId,
                PlacedByUserId = userId,
                BidAmountPerUnit = request.BidAmountPerUnit,
                TotalBidAmount = request.BidAmountPerUnit * request.Quantity,
                Quantity = request.Quantity,
                IsAnonymous = request.IsAnonymous,
                Status = BidStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            _context.Bids.Add(bid);

            try
            {
                await _context.SaveChangesAsync();

                var company = await _context.Companies.FindAsync(companyId);
                var displayName = request.IsAnonymous ? "Anonymous" : (company?.CompanyName ?? "Unknown");

                // Notify the previous bid owner they were outbid
                foreach (var prev in previousBids)
                {
                    await _notificationService.CreateNotificationAsync(
                        prev.PlacedByUserId,
                        "You've been outbid",
                        $"Your bid on '{listing.Title}' has been outbid. New price: {request.BidAmountPerUnit:C}",
                        "Listing",
                        listing.Id);
                }

                return new BidResultDto
                {
                    Success = true,
                    Message = "Bid placed successfully.",
                    DisplayBiddersName = displayName,
                    NewPrice = request.BidAmountPerUnit,
                    NewBidId = bid.Id,
                    NewBidCount = listing.BidCount
                };
            }
            catch (DbUpdateConcurrencyException)
            {
                return Fail("A concurrency error occurred. Please try again.");
            }
        }

        // ─── Quick Bid (from Marketplace Card) ───────────────────────────────────

        public async Task<BidResultDto> PlaceQuickBidAsync(int userId, int companyId, QuickBidDto request)
        {
            var listing = await _context.Listings.FindAsync(request.ListingId);
            if (listing == null)
                return Fail("Listing not found.");

            // Delegate to full PlaceBidAsync using the listing's minimum quantity
            var fullBid = new PlaceBidDto
            {
                ListingId = request.ListingId,
                BidAmountPerUnit = request.BidAmountPerUnit,
                TotalBidAmount = request.BidAmountPerUnit * listing.MinOrderQuantity,
                Quantity = listing.MinOrderQuantity,
                IsAnonymous = request.IsAnonymous
            };

            return await PlaceBidAsync(userId, companyId, fullBid);
        }

        // ─── Get Bids (basic result) ──────────────────────────────────────────────

        public async Task<IEnumerable<BidResultDto>> GetBidsForListingAsync(int listingId)
        {
            var bids = await _context.Bids
                .Include(b => b.BuyerCompany)
                .Where(b => b.ListingId == listingId)
                .OrderByDescending(b => b.BidAmountPerUnit)
                .ToListAsync();

            return bids.Select(b => new BidResultDto
            {
                Success = true,
                Message = "Bid retrieved",
                DisplayBiddersName = b.IsAnonymous ? "Anonymous" : b.BuyerCompany.CompanyName,
                NewPrice = b.BidAmountPerUnit
            });
        }

        // ─── Live Bids (full detail for bidding room) ─────────────────────────────

        public async Task<IEnumerable<BidDetailDto>> GetLiveBidsAsync(int listingId)
        {
            var bids = await _context.Bids
                .Include(b => b.BuyerCompany)
                .Where(b => b.ListingId == listingId)
                .OrderByDescending(b => b.BidAmountPerUnit)
                .Take(10)
                .ToListAsync();

            return bids.Select(MapToBidDetailDto);
        }

        // ─── Single Bid Detail ────────────────────────────────────────────────────

        public async Task<BidDetailDto?> GetBidDetailAsync(int bidId)
        {
            var bid = await _context.Bids
                .Include(b => b.BuyerCompany)
                .FirstOrDefaultAsync(b => b.Id == bidId);

            return bid == null ? null : MapToBidDetailDto(bid);
        }

        // ─── Delete / Cancel Bid ──────────────────────────────────────────────────

        public async Task<bool> DeleteBidAsync(int bidId, int companyId)
        {
            var bid = await _context.Bids.FindAsync(bidId);

            if (bid == null || bid.BuyerCompanyId != companyId)
                return false;

            bid.Status = BidStatus.Cancelled;
            await _context.SaveChangesAsync();
            return true;
        }

        // ─── Private Helpers ──────────────────────────────────────────────────────

        private static BidResultDto Fail(string message) =>
            new BidResultDto { Success = false, Message = message };

        private static BidDetailDto MapToBidDetailDto(Bids b) => new BidDetailDto
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
        };
    }
}
