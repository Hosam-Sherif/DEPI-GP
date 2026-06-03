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
                .Where(o => o.seller_company_id == companyId)
                .ToListAsync();

            var thisMonth = salesOrders.Where(o => o.order_date >= thisMonthStart).ToList();
            var lastMonth = salesOrders.Where(o => o.order_date >= lastMonthStart && o.order_date < thisMonthStart).ToList();

            var thisMonthRevenue = thisMonth.Sum(o => o.agreed_quantity * o.agreed_unit_price);
            var lastMonthRevenue = lastMonth.Sum(o => o.agreed_quantity * o.agreed_unit_price);
            var revenueGrowth = lastMonthRevenue == 0 ? 100 :
                Math.Round((double)(thisMonthRevenue - lastMonthRevenue) / (double)lastMonthRevenue * 100, 2);

            return Ok(new
            {
                company_id = companyId,
                total_orders = salesOrders.Count,
                total_revenue = Math.Round(salesOrders.Sum(o => o.agreed_quantity * o.agreed_unit_price), 2),
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
                .Where(o => o.seller_company_id == companyId && o.order_date >= fromDate)
                .GroupBy(o => new { Year = o.order_date.Year, Month = o.order_date.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalOrders = g.Count(),
                    TotalRevenue = Math.Round(g.Sum(o => o.agreed_quantity * o.agreed_unit_price), 2),
                    TotalQuantity = g.Sum(o => o.agreed_quantity),
                    AverageOrderValue = Math.Round(g.Average(o => o.agreed_quantity * o.agreed_unit_price), 2)
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
                .Where(o => o.seller_company_id == companyId)
                .GroupBy(o => new
                {
                    CategoryId = o.Bid.Listing.category_id,
                    CategoryName = o.Bid.Listing.Category.category_name
                })
                .Select(g => new
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName,
                    TotalOrders = g.Count(),
                    TotalRevenue = Math.Round(g.Sum(o => o.agreed_quantity * o.agreed_unit_price), 2),
                    TotalQuantitySold = g.Sum(o => o.agreed_quantity),
                    AveragePrice = Math.Round(g.Average(o => o.agreed_unit_price), 2)
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
                .Where(o => o.seller_company_id == companyId)
                .GroupBy(o => new { o.buyer_company_id, CompanyName = o.BuyerCompany.company_name, City = o.BuyerCompany.city })
                .Select(g => new
                {
                    BuyerCompanyId = g.Key.buyer_company_id,
                    BuyerName = g.Key.CompanyName,
                    City = g.Key.City,
                    TotalOrders = g.Count(),
                    TotalSpent = Math.Round(g.Sum(o => o.agreed_quantity * o.agreed_unit_price), 2)
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
                .Where(l => l.company_id == companyId && l.start_date <= now && l.end_date >= now)
                .CountAsync();

            var closedListings = await _context.Listings
                .Where(l => l.company_id == companyId && l.end_date < now)
                .CountAsync();

            var pendingOrders = await _context.Orders
                .Where(o => o.seller_company_id == companyId)
                .CountAsync();

            var totalBidsReceived = await _context.Bids
                .Include(b => b.Listing)
                .Where(b => b.Listing.company_id == companyId)
                .CountAsync();

            var recentBids = await _context.Bids
                .Include(b => b.Listing)
                .Include(b => b.BuyerCompany)
                .Where(b => b.Listing.company_id == companyId)
                .OrderByDescending(b => b.bid_time)
                .Take(5)
                .Select(b => new
                {
                    b.Id,
                    ListingTitle = b.Listing.title,
                    BidderName = b.is_anonymous ? b.anonymous_name : b.BuyerCompany.company_name,
                    b.bid_amount_per_unit,
                    b.bid_time
                })
                .ToListAsync();

            var recentOrders = await _context.Orders
                .Include(o => o.BuyerCompany)
                .Include(o => o.Bid)
                    .ThenInclude(b => b.Listing)
                .Where(o => o.seller_company_id == companyId)
                .OrderByDescending(o => o.order_date)
                .Take(5)
                .Select(o => new
                {
                    o.Id,
                    BuyerName = o.BuyerCompany.company_name,
                    ListingTitle = o.Bid.Listing.title,
                    o.agreed_quantity,
                    o.agreed_unit_price,
                    TotalValue = o.agreed_quantity * o.agreed_unit_price,
                    o.order_date
                })
                .ToListAsync();

            return Ok(new
            {
                company_id = companyId,
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
                .Where(l => l.company_id == companyId && l.start_date <= now && l.end_date >= now)
                .Select(l => new
                {
                    l.ID,
                    l.title,
                    CategoryName = l.Category.category_name,
                    l.available_quantity,
                    l.current_price,
                    l.end_date,
                    TimeRemaining = l.end_date - now,
                    TotalBids = l.Bids.Count,
                    TopBid = l.Bids.Any() ? l.Bids.Max(b => b.bid_amount_per_unit) : 0
                })
                .ToListAsync();

            return Ok(new
            {
                company_id = companyId,
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
                .Where(b => b.Listing.company_id == companyId)
                .OrderByDescending(b => b.bid_time)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new
                {
                    Type = "Bid",
                    Description = $"Bid on {b.Listing.title}",
                    Actor = b.is_anonymous ? b.anonymous_name : b.BuyerCompany.company_name,
                    Amount = b.bid_amount_per_unit,
                    Timestamp = b.bid_time
                })
                .ToListAsync();

            var orders = await _context.Orders
                .Include(o => o.BuyerCompany)
                .Include(o => o.Bid)
                    .ThenInclude(b => b.Listing)
                .Where(o => o.seller_company_id == companyId)
                .OrderByDescending(o => o.order_date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new
                {
                    Type = "Order",
                    Description = $"Purchase order for {o.Bid.Listing.title}",
                    Actor = o.BuyerCompany.company_name,
                    Amount = o.agreed_quantity * o.agreed_unit_price,
                    Timestamp = o.order_date
                })
                .ToListAsync();

            var activityLog = bids.Cast<object>()
                .Concat(orders.Cast<object>())
                .OrderByDescending(x => ((dynamic)x).Timestamp)
                .Take(pageSize)
                .ToList();

            return Ok(new
            {
                company_id = companyId,
                page,
                page_size = pageSize,
                activities = activityLog
            });
        }
    }
}