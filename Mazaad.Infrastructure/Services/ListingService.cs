using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mazaad.Application.Common;
using Mazaad.Application.DTOs;
using Mazaad.Application.Interfaces.Services;
using Mazaad.Domain.Enums;
using Mazaad.Domain.Models;
using Mazaad.Infrastructure.Hubs;
using Mazaad.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Mazaad.Infrastructure.Services
{
    public class ListingService : IListingService
    {
        private readonly AppDbContext _context;
        private readonly IImageService _imageService;
        private readonly IHubContext<AuctionHub> _hubContext;
        private readonly ISecurityLogService _securityLog;

        public ListingService(
            AppDbContext context,
            IImageService imageService,
            IHubContext<AuctionHub> hubContext,
            ISecurityLogService securityLog)
        {
            _context = context;
            _imageService = imageService;
            _hubContext = hubContext;
            _securityLog = securityLog;
        }

        // ─── Marketplace Grid ──────────────────────────────────────────────────────

        public async Task<PagedResultDto<ListingCardDto>> GetListingsAsync(ListingFilterDto filter)
        {
            var query = BuildFilteredQuery(filter);
            return await ToPagedCardResultAsync(query, filter.PageNumber, filter.PageSize);
        }

        // ─── Dashboard: My Listings ────────────────────────────────────────────────

        public async Task<PagedResultDto<ListingCardDto>> GetMyListingsAsync(int companyId, ListingFilterDto filter)
        {
            var query = BuildFilteredQuery(filter, companyId);
            return await ToPagedCardResultAsync(query, filter.PageNumber, filter.PageSize);
        }

        // ─── Single Listing Summary ────────────────────────────────────────────────

        public async Task<ListingResponseDto?> GetListingByIdAsync(int id)
        {
            var listing = await _context.Listings.FindAsync(id);
            if (listing == null || listing.IsDeleted) return null;
            if (listing.Status == ListingStatus.PendingApproval || listing.Status == ListingStatus.Rejected) return null;

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
                .Include(l => l.Bids)
                    .ThenInclude(b => b.User)
                .FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);

            if (listing == null) return null;
            if (listing.Status == ListingStatus.PendingApproval || listing.Status == ListingStatus.Rejected) return null;

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
                UnitOfMeasure = listing.UnitOfMeasure,
                PurityPercentage = listing.PurityPercentage,
                BaseCurrency = listing.BaseCurrency,
                CurrentHighestBid = listing.CurrentHighestBid,
                BidCount = listing.BidCount,
                Status = listing.Status,
                Condition = listing.Condition,
                ImageUrl = listing.ImageUrl,
                Location = listing.Location,
                DueDiligenceUrls = listing.DueDiligenceUrls,
                StartDate = DateTime.SpecifyKind(listing.StartDate, DateTimeKind.Utc), // ← Fix
                EndDate = DateTime.SpecifyKind(listing.EndDate, DateTimeKind.Utc), // ← Fix
                WinningBidId = listing.Bids
                    .OrderByDescending(b => b.BidAmountPerUnit)
                    .Select(b => (int?)b.Id)
                    .FirstOrDefault(),
                TopBids = listing.Bids.Select(b => new BidDetailDto
                {
                    Id = b.Id,
                    ListingId = b.ListingId,
                    BuyerCompanyId = b.BuyerCompanyId,
                    DisplayBidderName = b.IsAnonymous
                        ? "Anonymous"
                        : (b.BuyerCompany?.CompanyName ?? b.User?.FullName ?? "Unknown"),
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
            var category = await _context.MaterialCategories.FindAsync(request.CategoryId);
            var unitOfMeasure = category?.UnitOfMeasure ?? request.UnitOfMeasure ?? "kg";

            var listing = new Listings
            {
                CompanyId = companyId,
                CategoryId = request.CategoryId,
                Title = request.Title,
                Description = request.Description,
                MinOrderQuantity = request.MinOrderQuantity,
                AvailableQuantity = request.AvailableQuantity,
                UnitOfMeasure = unitOfMeasure,
                PurityPercentage = request.PurityPercentage,
                BaseCurrency = request.BaseCurrency,
                StartDate = EnsureUtc(request.StartDate),
                EndDate = EnsureUtc(request.EndDate),
                CurrentHighestBid = request.StartingPrice,
                ImageUrl = "",
                // كل listing جديد يبدأ PendingApproval ومش هيظهر أو ينزل الا لما SuperAdmin يوافق عليه
                Status = ListingStatus.PendingApproval,
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

            var category = await _context.MaterialCategories.FindAsync(request.CategoryId);
            var unitOfMeasure = category?.UnitOfMeasure ?? listing.UnitOfMeasure;

            listing.CategoryId = request.CategoryId;
            listing.Title = request.Title;
            listing.Description = request.Description;
            listing.MinOrderQuantity = request.MinOrderQuantity;
            listing.AvailableQuantity = request.AvailableQuantity;
            listing.UnitOfMeasure = unitOfMeasure;
            listing.PurityPercentage = request.PurityPercentage;
            listing.BaseCurrency = request.BaseCurrency;
            listing.StartDate = EnsureUtc(request.StartDate);
            listing.EndDate = EnsureUtc(request.EndDate);
            listing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToResponseDto(listing);
        }

        // ─── Upload Image ─────────────────────────────────────────────────────────

        public async Task<ListingResponseDto?> UploadListingImageAsync(int listingId, int companyId, IFormFile image)
        {
            var listing = await _context.Listings
                .FirstOrDefaultAsync(l => l.Id == listingId
                                       && l.CompanyId == companyId
                                       && !l.IsDeleted);

            if (listing == null) return null;

            using var stream = image.OpenReadStream();
            var imageUrl = await _imageService.UploadImageAsync(
                stream,
                image.FileName,
                $"mazzad/listings/{companyId}"
            );

            listing.ImageUrl = imageUrl;
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

        // ─── Cancel Listing ───────────────────────────────────────────────────────

        public async Task<bool> CancelListingAsync(int id, int companyId)
        {
            var listing = await _context.Listings.FindAsync(id);

            if (listing == null || listing.IsDeleted || listing.CompanyId != companyId)
                return false;

            if (listing.Status == ListingStatus.Cancelled)
                return false;

            listing.Status = ListingStatus.Cancelled;
            listing.UpdatedAt = DateTime.UtcNow;

            var bids = await _context.Bids
                .Where(b => b.ListingId == id && b.Status != BidStatus.Cancelled)
                .ToListAsync();

            foreach (var bid in bids)
                bid.Status = BidStatus.Cancelled;

            await _context.SaveChangesAsync();
            return true;
        }

        // ─── End Auction Now ──────────────────────────────────────────────────────

        public async Task<EndAuctionResultDto?> EndListingNowAsync(int id, int companyId)
        {
            var listing = await _context.Listings
                .Include(l => l.Bids)
                    .ThenInclude(b => b.BuyerCompany)
                .Include(l => l.Bids)
                    .ThenInclude(b => b.User)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (listing == null || listing.IsDeleted || listing.CompanyId != companyId)
                return null;

            if (listing.Status == ListingStatus.Ended || listing.Status == ListingStatus.Cancelled)
                return null;

            var now = DateTime.UtcNow;
            listing.Status = ListingStatus.Ended;
            listing.EndDate = now;
            listing.UpdatedAt = now;

            var winningBid = listing.Bids
                .Where(b => b.Status != BidStatus.Cancelled)
                .OrderByDescending(b => b.BidAmountPerUnit)
                .FirstOrDefault();

            var result = new EndAuctionResultDto
            {
                ListingId = listing.Id,
                Title = listing.Title,
                Status = listing.Status,
                EndDate = listing.EndDate,
                HasWinner = winningBid != null
            };

            if (winningBid != null)
            {
                result.WinningBidId = winningBid.Id;
                result.WinnerDisplayName = winningBid.IsAnonymous
                    ? "Anonymous"
                    : (winningBid.BuyerCompany?.CompanyName ?? winningBid.User?.FullName ?? "Unknown");
                result.WinningBidAmountPerUnit = winningBid.BidAmountPerUnit;
                result.WinningTotalAmount = winningBid.TotalBidAmount;
                result.WinningQuantity = winningBid.Quantity;
            }

            await _context.SaveChangesAsync();

            var payload = new
            {
                listingId = result.ListingId,
                title = result.Title,
                endDate = result.EndDate,
                hasWinner = result.HasWinner,
                winnerDisplayName = result.WinnerDisplayName,
                winningBidAmountPerUnit = result.WinningBidAmountPerUnit
            };

            await _hubContext.Clients.Group($"auction-{id}").SendAsync("AuctionEnded", payload);
            await _hubContext.Clients.Group($"listing-{id}").SendAsync("AuctionEnded", payload);

            return result;
        }

        // ─── SuperAdmin: Approval Queue ─────────────────────────────────────────────

        public async Task<IEnumerable<PendingListingDto>> GetPendingListingsAsync()
        {
            var listings = await _context.Listings
                .Include(l => l.Category)
                .Include(l => l.Company)
                .Where(l => !l.IsDeleted && l.Status == ListingStatus.PendingApproval)
                .OrderBy(l => l.CreatedAt)
                .ToListAsync();

            return listings.Select(l => new PendingListingDto
            {
                Id = l.Id,
                Title = l.Title,
                CompanyName = l.Company.CompanyName,
                CategoryName = l.Category.CategoryName,
                StartingPrice = l.CurrentHighestBid,
                BaseCurrency = l.BaseCurrency,
                UnitOfMeasure = l.UnitOfMeasure,
                StartDate = DateTime.SpecifyKind(l.StartDate, DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(l.EndDate, DateTimeKind.Utc),
                CreatedAt = l.CreatedAt
            });
        }

        // ─── SuperAdmin: Approve or Reject ──────────────────────────────────────────

        public async Task<Result> ApproveListingAsync(int listingId, int adminUserId, ApproveListingDto dto, string ipAddress)
        {
            var listing = await _context.Listings.FindAsync(listingId);
            if (listing == null || listing.IsDeleted)
                return Result.Failure("Listing not found.");

            if (listing.Status != ListingStatus.PendingApproval)
                return Result.Failure("Listing is not pending approval.");

            if (!dto.Approved && string.IsNullOrWhiteSpace(dto.RejectionReason))
                return Result.Failure("Rejection reason is required.");

            var now = DateTime.UtcNow;

            listing.Status = dto.Approved
                ? (EnsureUtc(listing.StartDate) > now ? ListingStatus.Upcoming : ListingStatus.Active)
                : ListingStatus.Rejected;

            listing.ApprovedByUserId = adminUserId;
            listing.ApprovedAt = now;
            listing.RejectionReason = dto.Approved ? null : dto.RejectionReason;
            listing.UpdatedAt = now;

            await _context.SaveChangesAsync();

            await _securityLog.LogAsync(
                dto.Approved
                    ? SecurityEventType.ListingApproved
                    : SecurityEventType.ListingRejected,
                success: true,
                ipAddress: ipAddress,
                userId: adminUserId,
                details: $"Listing: {listing.Title} (Id={listing.Id})" +
                         (dto.Approved ? "" : $" | Reason: {dto.RejectionReason}"));

            return Result.Success();
        }

        // ─── Private Helpers ──────────────────────────────────────────────────────

        private static DateTime EnsureUtc(DateTime dt) => dt.Kind switch
        {
            DateTimeKind.Utc => dt,
            DateTimeKind.Local => dt.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
        };

        private IQueryable<Listings> BuildFilteredQuery(ListingFilterDto filter, int? companyId = null)
        {
            var query = _context.Listings
                .Include(l => l.Category)
                .Include(l => l.Company)
                .Where(l => !l.IsDeleted);

            if (companyId.HasValue)
            {
                // "My Listings" dashboard: company should still see its own pending/rejected listings
                query = query.Where(l => l.CompanyId == companyId.Value);
            }
            else
            {
                // Public marketplace: never show a listing that hasn't been approved yet
                query = query.Where(l => l.Status != ListingStatus.PendingApproval
                                       && l.Status != ListingStatus.Rejected);
            }

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

            return query;
        }

        private static async Task<PagedResultDto<ListingCardDto>> ToPagedCardResultAsync(
            IQueryable<Listings> query, int pageNumber, int pageSize)
        {
            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
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
                    UnitOfMeasure = l.UnitOfMeasure,
                    EndDate = l.EndDate,
                    WinningBidId = l.Bids
                        .OrderByDescending(b => b.BidAmountPerUnit)
                        .Select(b => (int?)b.Id)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return new PagedResultDto<ListingCardDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        // ── Fix: SpecifyKind على StartDate و EndDate + إضافة StartingPrice ────────
        private static ListingResponseDto MapToResponseDto(Listings listing) => new ListingResponseDto
        {
            Id = listing.Id,
            CompanyId = listing.CompanyId,
            CategoryId = listing.CategoryId,
            Title = listing.Title,
            Description = listing.Description,
            MinOrderQuantity = listing.MinOrderQuantity,
            AvailableQuantity = listing.AvailableQuantity,
            UnitOfMeasure = listing.UnitOfMeasure,
            PurityPercentage = listing.PurityPercentage,
            BaseCurrency = listing.BaseCurrency,
            StartDate = DateTime.SpecifyKind(listing.StartDate, DateTimeKind.Utc), // ← Fix UTC
            EndDate = DateTime.SpecifyKind(listing.EndDate, DateTimeKind.Utc), // ← Fix UTC
            StartingPrice = listing.CurrentHighestBid,                                 // ← مضاف
            CurrentHighestBid = listing.CurrentHighestBid,
            ImageUrl = listing.ImageUrl
        };
    }
}