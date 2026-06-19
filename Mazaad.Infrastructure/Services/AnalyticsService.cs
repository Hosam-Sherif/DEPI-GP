using Mazaad.Application.DTOs.Analytics;
using Mazaad.Application.Interfaces.Services;
using Mazaad.Domain.Enums;
using Mazaad.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mazaad.Infrastructure.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly AppDbContext _db;

        public AnalyticsService(AppDbContext db)
        {
            _db = db;
        }

        // ─── 1. Asset Value Index ─────────────────────────────────────────────────

        public async Task<IEnumerable<AssetValueIndexDto>> GetAssetValueIndexAsync()
        {
            var result = await _db.Listings
                .Where(l => !l.IsDeleted && l.Status == ListingStatus.Active)
                .Include(l => l.Category)
                .Include(l => l.Bids)
                .GroupBy(l => new { l.CategoryId, l.Category.CategoryName, l.BaseCurrency })
                .Select(g => new AssetValueIndexDto
                {
                    CategoryName = g.Key.CategoryName,
                    BaseCurrency = g.Key.BaseCurrency,
                    ActiveListingsCount = g.Count(),
                    AverageBidPrice = g.SelectMany(l => l.Bids)
                                         .Where(b => b.Status == BidStatus.Active)
                                         .Any()
                                       ? g.SelectMany(l => l.Bids)
                                           .Where(b => b.Status == BidStatus.Active)
                                           .Average(b => b.BidAmountPerUnit)
                                       : 0,
                    HighestBid = g.SelectMany(l => l.Bids)
                                         .Where(b => b.Status == BidStatus.Active)
                                         .Any()
                                       ? g.SelectMany(l => l.Bids)
                                           .Where(b => b.Status == BidStatus.Active)
                                           .Max(b => b.BidAmountPerUnit)
                                       : 0,
                    LowestBid = g.SelectMany(l => l.Bids)
                                         .Where(b => b.Status == BidStatus.Active)
                                         .Any()
                                       ? g.SelectMany(l => l.Bids)
                                           .Where(b => b.Status == BidStatus.Active)
                                           .Min(b => b.BidAmountPerUnit)
                                       : 0,
                })
                .OrderByDescending(x => x.ActiveListingsCount)
                .ToListAsync();

            return result;
        }

        // ─── 2. Regional Demand Heatmap ───────────────────────────────────────────

        public async Task<IEnumerable<RegionalDemandDto>> GetRegionalDemandHeatmapAsync()
        {
            // Listings grouped by city (Location field on Listing)
            var listingsByRegion = await _db.Listings
                .Where(l => !l.IsDeleted && !string.IsNullOrEmpty(l.Location))
                .GroupBy(l => l.Location)
                .Select(g => new
                {
                    Region = g.Key,
                    ActiveListings = g.Count(l => l.Status == ListingStatus.Active),
                    TotalBids = g.Sum(l => l.BidCount),
                })
                .ToListAsync();

            // Orders grouped by seller city
            var ordersByRegion = await _db.Orders
                .Include(o => o.SellerCompany)
                .Where(o => o.Status == OrderStatus.Completed)
                .GroupBy(o => o.SellerCompany.City)
                .Select(g => new
                {
                    Region = g.Key,
                    TotalOrders = g.Count(),
                    TotalValue = g.Sum(o => o.TotalAmount),
                })
                .ToListAsync();

            // Merge both datasets by region
            var regions = listingsByRegion
                .Select(l => l.Region)
                .Union(ordersByRegion.Select(o => o.Region))
                .Distinct()
                .ToList();

            var merged = regions.Select(region =>
            {
                var listing = listingsByRegion.FirstOrDefault(x => x.Region == region);
                var order = ordersByRegion.FirstOrDefault(x => x.Region == region);
                return new
                {
                    Region = region,
                    TotalBids = listing?.TotalBids ?? 0,
                    TotalOrders = order?.TotalOrders ?? 0,
                    TotalOrderValue = order?.TotalValue ?? 0,
                    ActiveListings = listing?.ActiveListings ?? 0,
                    RawScore = (listing?.TotalBids ?? 0) + ((order?.TotalOrders ?? 0) * 3),
                };
            }).ToList();

            // Normalize score to 0–100
            int maxScore = merged.Any() ? merged.Max(x => x.RawScore) : 1;

            var result = merged
                .Select(x => new RegionalDemandDto
                {
                    Region = x.Region,
                    TotalBids = x.TotalBids,
                    TotalOrders = x.TotalOrders,
                    TotalOrderValue = x.TotalOrderValue,
                    ActiveListings = x.ActiveListings,
                    DemandScore = maxScore == 0 ? 0 : (int)Math.Round((double)x.RawScore / maxScore * 100),
                })
                .OrderByDescending(x => x.DemandScore)
                .ToList();

            return result;
        }

        // ─── 3. Recent Benchmarks ─────────────────────────────────────────────────

        public async Task<IEnumerable<RecentBenchmarkDto>> GetRecentBenchmarksAsync(int count = 10)
        {
            var result = await _db.Orders
                .Where(o => o.Status == OrderStatus.Completed)
                .Include(o => o.Bid)
                    .ThenInclude(b => b.Listing)
                        .ThenInclude(l => l.Category)
                .Include(o => o.SellerCompany)
                .Include(o => o.BuyerCompany)
                .OrderByDescending(o => o.OrderDate)
                .Take(count)
                .Select(o => new RecentBenchmarkDto
                {
                    OrderId = o.Id,
                    ListingTitle = o.Bid.Listing.Title,
                    CategoryName = o.Bid.Listing.Category.CategoryName,
                    SellerCompany = o.SellerCompany.CompanyName,
                    BuyerCompany = o.BuyerCompany.CompanyName,
                    AgreedUnitPrice = o.AgreedUnitPrice,
                    AgreedQuantity = o.AgreedQuantity,
                    TotalAmount = o.TotalAmount,
                    BaseCurrency = o.Bid.Listing.BaseCurrency,
                    OrderDate = o.OrderDate,
                })
                .ToListAsync();

            return result;
        }

        // ─── 4. Momentum Movers ───────────────────────────────────────────────────

        public async Task<IEnumerable<MomentumMoverDto>> GetMomentumMoversAsync(int top = 10)
        {
            var now = DateTime.UtcNow;
            var last7Days = now.AddDays(-7);
            var prev7Days = now.AddDays(-14);

            var result = await _db.Listings
                .Where(l => !l.IsDeleted && l.Status == ListingStatus.Active)
                .Include(l => l.Category)
                .Include(l => l.Bids)
                .Select(l => new
                {
                    l.Id,
                    l.Title,
                    CategoryName = l.Category.CategoryName,
                    l.Location,
                    l.CurrentHighestBid,
                    l.BaseCurrency,
                    l.BidCount,
                    l.EndDate,
                    BidsLast7Days = l.Bids.Count(b => b.CreatedAt >= last7Days),
                    BidsPrev7Days = l.Bids.Count(b => b.CreatedAt >= prev7Days && b.CreatedAt < last7Days),
                })
                .Where(x => x.BidsLast7Days > 0)
                .OrderByDescending(x => x.BidsLast7Days)
                .Take(top)
                .ToListAsync();

            return result.Select(x => new MomentumMoverDto
            {
                ListingId = x.Id,
                Title = x.Title,
                CategoryName = x.CategoryName,
                Location = x.Location,
                CurrentHighestBid = x.CurrentHighestBid,
                BaseCurrency = x.BaseCurrency,
                BidsLast7Days = x.BidsLast7Days,
                TotalBids = x.BidCount,
                MomentumGrowthPercent = x.BidsPrev7Days == 0
                    ? 100
                    : Math.Round((decimal)(x.BidsLast7Days - x.BidsPrev7Days) / x.BidsPrev7Days * 100, 2),
                EndDate = x.EndDate,
            });
        }
    }
}