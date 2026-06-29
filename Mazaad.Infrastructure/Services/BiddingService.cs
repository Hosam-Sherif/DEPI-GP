using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mazaad.Application.DTOs;
using Mazaad.Application.Interfaces.Services;
using Mazaad.Domain.Enums;
using Mazaad.Domain.Models;
using Mazaad.Infrastructure.Hubs;
using Mazaad.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Mazaad.Infrastructure.Services
{
    public class BiddingService : IBiddingService
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<AuctionHub> _hubContext;

        public BiddingService(
            AppDbContext context,
            INotificationService notificationService,
            IHubContext<AuctionHub> hubContext)
        {
            _context = context;
            _notificationService = notificationService;
            _hubContext = hubContext;
        }

        // ─── Place Full Bid ───────────────────────────────────────────────────────

        public async Task<BidResultDto> PlaceBidAsync(int userId, int companyId, PlaceBidDto request)
        {
            if (request.BidAmountPerUnit <= 0)
                return Fail("Bid amount must be greater than zero.");

            if (request.Quantity <= 0)
                return Fail("Quantity must be greater than zero.");

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

            var serverComputedTotal = request.BidAmountPerUnit * request.Quantity;

            var previousActiveBids = await _context.Bids
                .Where(b => b.ListingId == request.ListingId && b.Status == BidStatus.Active)
                .ToListAsync();
            foreach (var prev in previousActiveBids)
                prev.Status = BidStatus.Outbid;

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
                TotalBidAmount = serverComputedTotal,
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

                foreach (var prev in previousActiveBids)
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

        // ─── Quick Bid ────────────────────────────────────────────────────────────

        public async Task<BidResultDto> PlaceQuickBidAsync(int userId, int companyId, QuickBidDto request)
        {
            if (request.BidAmountPerUnit <= 0)
                return Fail("Bid amount must be greater than zero.");

            var listing = await _context.Listings.FindAsync(request.ListingId);
            if (listing == null)
                return Fail("Listing not found.");

            var topBid = await _context.Bids
                .Where(b => b.ListingId == request.ListingId && b.Status == BidStatus.Active)
                .OrderByDescending(b => b.BidAmountPerUnit)
                .FirstOrDefaultAsync();

            var quantity = topBid?.Quantity ?? listing.MinOrderQuantity;

            var fullBid = new PlaceBidDto
            {
                ListingId = request.ListingId,
                BidAmountPerUnit = request.BidAmountPerUnit,
                Quantity = quantity,
                IsAnonymous = request.IsAnonymous
            };

            return await PlaceBidAsync(userId, companyId, fullBid);
        }

        // ─── Get Bids for Listing ─────────────────────────────────────────────────

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

        // ─── Live Bids ────────────────────────────────────────────────────────────

        public async Task<IEnumerable<BidDetailDto>> GetLiveBidsAsync(int listingId)
        {
            var bids = await _context.Bids
                .Include(b => b.BuyerCompany)
                .Where(b => b.ListingId == listingId && b.Status != BidStatus.Cancelled)
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

        // ─── My Bids (by company) ─────────────────────────────────────────────────

        public async Task<IEnumerable<BidDetailDto>> GetBidsByCompanyAsync(int companyId)
        {
            var bids = await _context.Bids
                .Include(b => b.BuyerCompany)
                .Include(b => b.Listing)
                .Where(b => b.BuyerCompanyId == companyId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return bids.Select(b =>
            {
                var dto = MapToBidDetailDto(b);

                var isHighestBid = b.Listing != null &&
                                   b.Listing.CurrentHighestBid == b.BidAmountPerUnit;

                // فاز: المزاد خلص (Status = Ended) وهو الأعلى
                if (isHighestBid && b.Listing!.Status == ListingStatus.Ended)
                {
                    dto.Status = BidStatus.Won;
                }
                // في الصدارة: المزاد لسه شغال وهو الأعلى
                else if (isHighestBid && b.Listing!.Status == ListingStatus.Active)
                {
                    dto.Status = BidStatus.Winning;
                }

                return dto;
            });
        }

        // ─── Delete / Cancel Bid ──────────────────────────────────────────────────

        public async Task<bool> DeleteBidAsync(int bidId, int companyId)
        {
            var bid = await _context.Bids.FindAsync(bidId);

            if (bid == null || bid.BuyerCompanyId != companyId)
                return false;

            if (bid.Status == BidStatus.Cancelled)
                return false;

            var wasActiveTopBid = bid.Status == BidStatus.Active;
            bid.Status = BidStatus.Cancelled;

            var listing = await _context.Listings.FindAsync(bid.ListingId);

            if (wasActiveTopBid && listing != null)
            {
                var nextTopBid = await _context.Bids
                    .Where(b => b.ListingId == bid.ListingId
                             && b.Id != bid.Id
                             && b.Status == BidStatus.Outbid)
                    .OrderByDescending(b => b.BidAmountPerUnit)
                    .FirstOrDefaultAsync();

                if (nextTopBid != null)
                {
                    nextTopBid.Status = BidStatus.Active;
                    listing.CurrentHighestBid = nextTopBid.BidAmountPerUnit;
                }
                else
                {
                    listing.CurrentHighestBid = 0;
                }
            }

            await _context.SaveChangesAsync();

            if (listing != null)
            {
                var liveBids = await GetLiveBidsAsync(listing.Id);

                await _hubContext.Clients
                    .Group($"auction-{listing.Id}")
                    .SendAsync("BidCancelled", new
                    {
                        ListingId = listing.Id,
                        CancelledBidId = bid.Id,
                        NewHighestBid = listing.CurrentHighestBid,
                        LiveBids = liveBids
                    });
            }

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