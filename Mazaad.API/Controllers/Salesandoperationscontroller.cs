using Mazaad.Domain.Models;
using Mazaad.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Mazaad.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesStatisticsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SalesStatisticsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("company/{companyId}/summary")]
        public async Task<IActionResult> GetSalesSummary(int companyId)
        {
            var now = DateTime.UtcNow;
            var thisMonthStart = new DateTime(now.Year, now.Month, 1);
            var lastMonthStart = thisMonthStart.AddMonths(-1);

            var salesOrders = await _context.Orders
                .Where(o => o.SellerCompanyId == companyId)
                .ToListAsync();

            var thisMonth = salesOrders.Where(o => o.OrderDate >= thisMonthStart).ToList();
            var lastMonth = salesOrders.Where(o => o.OrderDate >= lastMonthStart && o.OrderDate < thisMonthStart).ToList();

            var thisMonthRevenue = thisMonth.Sum(o => o.AgreedQuantity * o.AgreedUnitPrice);
            var lastMonthRevenue = lastMonth.Sum(o => o.AgreedQuantity * o.AgreedUnitPrice);
            var revenueGrowth = lastMonthRevenue == 0 ? 100 :
                Math.Round((double)(thisMonthRevenue - lastMonthRevenue) / (double)lastMonthRevenue * 100, 2);

            return Ok(new
            {
                CompanyId = companyId,
                total_orders = salesOrders.Count,
                total_revenue = Math.Round(salesOrders.Sum(o => o.AgreedQuantity * o.AgreedUnitPrice), 2),
                this_month = new
                {
                    orders = thisMonth.Count,
                    revenue = Math.Round(thisMonthRevenue, 2)
                },
                last_month = new
                {
                    orders = lastMonth.Count,
                    revenue = Math.Round(lastMonthRevenue, 2)
                },
                revenue_growth = revenueGrowth,
                trend = revenueGrowth > 0 ? "Up" : revenueGrowth < 0 ? "Down" : "Stable"
            });
        }

        [HttpGet("company/{companyId}/monthly")]
        public async Task<IActionResult> GetMonthlySales(int companyId, [FromQuery] int months = 12)
        {
            var fromDate = DateTime.UtcNow.AddMonths(-months);

            var result = await _context.Orders
                .Where(o => o.SellerCompanyId == companyId && o.OrderDate >= fromDate)
                .GroupBy(o => new { Year = o.OrderDate.Year, Month = o.OrderDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalOrders = g.Count(),
                    TotalRevenue = Math.Round(g.Sum(o => o.AgreedQuantity * o.AgreedUnitPrice), 2),
                    TotalQuantity = g.Sum(o => o.AgreedQuantity),
                    AverageOrderValue = Math.Round(g.Average(o => o.AgreedQuantity * o.AgreedUnitPrice), 2)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("company/{companyId}/top-products")]
        public async Task<IActionResult> GetTopProducts(int companyId, [FromQuery] int top = 5)
        {
            var result = await _context.Orders
                .Include(o => o.Bid)
                    .ThenInclude(b => b.Listing)
                        .ThenInclude(l => l.Category)
                .Where(o => o.SellerCompanyId == companyId)
                .GroupBy(o => new
                {
                    CategoryId = o.Bid.Listing.CategoryId,
                    CategoryName = o.Bid.Listing.Category.CategoryName
                })
                .Select(g => new
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName,
                    TotalOrders = g.Count(),
                    TotalRevenue = Math.Round(g.Sum(o => o.AgreedQuantity * o.AgreedUnitPrice), 2),
                    TotalQuantitySold = g.Sum(o => o.AgreedQuantity),
                    AveragePrice = Math.Round(g.Average(o => o.AgreedUnitPrice), 2)
                })
                .OrderByDescending(x => x.TotalRevenue)
                .Take(top)
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("company/{companyId}/top-buyers")]
        public async Task<IActionResult> GetTopBuyers(int companyId, [FromQuery] int top = 5)
        {
            var result = await _context.Orders
                .Include(o => o.BuyerCompany)
                .Where(o => o.SellerCompanyId == companyId)
                .GroupBy(o => new { o.BuyerCompanyId, CompanyName = o.BuyerCompany.CompanyName, City = o.BuyerCompany.City })
                .Select(g => new
                {
                    BuyerCompanyId = g.Key.BuyerCompanyId,
                    BuyerName = g.Key.CompanyName,
                    City = g.Key.City,
                    TotalOrders = g.Count(),
                    TotalSpent = Math.Round(g.Sum(o => o.AgreedQuantity * o.AgreedUnitPrice), 2)
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(top)
                .ToListAsync();

            return Ok(result);
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class OperationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OperationsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("company/{companyId}/dashboard")]
        public async Task<IActionResult> GetDashboard(int companyId)
        {
            var now = DateTime.UtcNow;

            var activeListings = await _context.Listings
                .Where(l => l.CompanyId == companyId && l.StartDate <= now && l.EndDate >= now)
                .CountAsync();

            var closedListings = await _context.Listings
                .Where(l => l.CompanyId == companyId && l.EndDate < now)
                .CountAsync();

            var pendingOrders = await _context.Orders
                .Where(o => o.SellerCompanyId == companyId)
                .CountAsync();

            var totalBidsReceived = await _context.Bids
                .Include(b => b.Listing)
                .Where(b => b.Listing.CompanyId == companyId)
                .CountAsync();

            var recentBids = await _context.Bids
                .Include(b => b.Listing)
                .Include(b => b.BuyerCompany)
                .Where(b => b.Listing.CompanyId == companyId)
                .OrderByDescending(b => b.CreatedAt)
                .Take(5)
                .Select(b => new
                {
                    b.Id,
                    ListingTitle = b.Listing.Title,
                    BidderName = b.IsAnonymous ? "Anonymous" : b.BuyerCompany.CompanyName,
                    b.BidAmountPerUnit,
                    b.CreatedAt
                })
                .ToListAsync();

            var recentOrders = await _context.Orders
                .Include(o => o.BuyerCompany)
                .Include(o => o.Bid)
                    .ThenInclude(b => b.Listing)
                .Where(o => o.SellerCompanyId == companyId)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Select(o => new
                {
                    o.Id,
                    BuyerName = o.BuyerCompany.CompanyName,
                    ListingTitle = o.Bid.Listing.Title,
                    o.AgreedQuantity,
                    o.AgreedUnitPrice,
                    TotalValue = o.AgreedQuantity * o.AgreedUnitPrice,
                    o.OrderDate
                })
                .ToListAsync();

            return Ok(new
            {
                CompanyId = companyId,
                timestamp = now,
                overview = new
                {
                    active_listings = activeListings,
                    closed_listings = closedListings,
                    total_orders = pendingOrders,
                    total_bids_received = totalBidsReceived
                },
                recent_bids = recentBids,
                recent_orders = recentOrders
            });
        }

        [HttpGet("company/{companyId}/active-auctions")]
        public async Task<IActionResult> GetActiveAuctions(int companyId)
        {
            var now = DateTime.UtcNow;

            var auctions = await _context.Listings
                .Include(l => l.Category)
                .Include(l => l.Bids)
                .Where(l => l.CompanyId == companyId && l.StartDate <= now && l.EndDate >= now)
                .Select(l => new
                {
                    l.Id,
                    l.Title,
                    CategoryName = l.Category.CategoryName,
                    l.AvailableQuantity,
                    l.CurrentHighestBid,
                    l.EndDate,
                    TimeRemaining = l.EndDate - now,
                    TotalBids = l.Bids.Count,
                    TopBid = l.Bids.Any() ? l.Bids.Max(b => b.BidAmountPerUnit) : 0
                })
                .ToListAsync();

            return Ok(new
            {
                CompanyId = companyId,
                active_auctions_count = auctions.Count,
                auctions
            });
        }

        [HttpGet("company/{companyId}/activity-log")]
        public async Task<IActionResult> GetActivityLog(int companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var bids = await _context.Bids
                .Include(b => b.Listing)
                .Include(b => b.BuyerCompany)
                .Where(b => b.Listing.CompanyId == companyId)
                .OrderByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new
                {
                    Type = "Bid",
                    Description = $"Bid on {b.Listing.Title}",
                    Actor = b.IsAnonymous ? "Anonymous" : b.BuyerCompany.CompanyName,
                    Amount = b.BidAmountPerUnit,
                    Timestamp = b.CreatedAt
                })
                .ToListAsync();

            var orders = await _context.Orders
                .Include(o => o.BuyerCompany)
                .Include(o => o.Bid)
                    .ThenInclude(b => b.Listing)
                .Where(o => o.SellerCompanyId == companyId)
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new
                {
                    Type = "Order",
                    Description = $"Purchase order for {o.Bid.Listing.Title}",
                    Actor = o.BuyerCompany.CompanyName,
                    Amount = o.AgreedQuantity * o.AgreedUnitPrice,
                    Timestamp = o.OrderDate
                })
                .ToListAsync();

            var activityLog = bids.Cast<object>()
                .Concat(orders.Cast<object>())
                .OrderByDescending(x => ((dynamic)x).Timestamp)
                .Take(pageSize)
                .ToList();

            return Ok(new
            {
                CompanyId = companyId,
                page,
                page_size = pageSize,
                activities = activityLog
            });
        }
    }
}
