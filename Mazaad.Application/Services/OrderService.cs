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
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public OrderService(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // ─── List Orders ──────────────────────────────────────────────────────────

        public async Task<IEnumerable<OrderResponseDto>> GetOrdersForCompanyAsync(int companyId)
        {
            var orders = await _context.Orders
                .Include(o => o.SellerCompany)
                .Include(o => o.BuyerCompany)
                .Include(o => o.Bid)
                .Where(o => o.SellerCompanyId == companyId || o.BuyerCompanyId == companyId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return orders.Select(o => MapToDto(o));
        }

        // ─── Single Order ─────────────────────────────────────────────────────────

        public async Task<OrderResponseDto?> GetOrderByIdAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.SellerCompany)
                .Include(o => o.BuyerCompany)
                .Include(o => o.Bid)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            return order == null ? null : MapToDto(order);
        }

        // ─── Finalize Order ───────────────────────────────────────────────────────

        public async Task<OrderResponseDto> FinalizeOrderAsync(int sellerCompanyId, FinalizeOrderDto request)
        {
            var bid = await _context.Bids
                .Include(b => b.Listing)
                .Include(b => b.BuyerCompany)
                .FirstOrDefaultAsync(b => b.Id == request.BidId);

            if (bid == null)
                throw new InvalidOperationException("Bid not found.");

            if (bid.Listing.CompanyId != sellerCompanyId)
                throw new UnauthorizedAccessException("You do not own this listing.");

            if (bid.Listing.EndDate > DateTime.UtcNow)
                throw new InvalidOperationException("Auction has not ended yet.");

            // Find the applicable commission policy
            var policy = await _context.CommissionPolicies
                .Where(p => p.Active && p.EffectiveFrom <= DateTime.UtcNow && p.EffectiveTo >= DateTime.UtcNow)
                .OrderByDescending(p => p.EffectiveFrom)
                .FirstOrDefaultAsync();

            if (policy == null)
                throw new InvalidOperationException("No active commission policy found.");

            var totalAmount = bid.BidAmountPerUnit * bid.Quantity;
            var platformFee = Math.Round(totalAmount * policy.CommissionRate / 100, 2);

            // Mark bid as won
            bid.Status = BidStatus.Won;
            bid.WinningBid = true;

            var order = new Orders
            {
                SellerCompanyId = sellerCompanyId,
                BuyerCompanyId = bid.BuyerCompanyId,
                BidId = bid.Id,
                AppliedPolicyId = policy.Id,
                AgreedQuantity = bid.Quantity,
                AgreedUnitPrice = bid.BidAmountPerUnit,
                TotalAmount = totalAmount,
                PlatformFee = platformFee,
                Status = OrderStatus.Confirmed,
                Notes = request.Notes,
                OrderDate = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Notify buyer
            await _notificationService.CreateNotificationAsync(
                bid.PlacedByUserId,
                "Congratulations! You won!",
                $"Your bid on '{bid.Listing.Title}' was accepted. Order #{order.Id} confirmed.",
                "Order",
                order.Id);

            return MapToDto(order, bid.BuyerCompany?.CompanyName ?? string.Empty);
        }

        // ─── Update Status ────────────────────────────────────────────────────────

        public async Task<bool> UpdateOrderStatusAsync(int orderId, int companyId, OrderStatus newStatus)
        {
            var order = await _context.Orders.FindAsync(orderId);

            if (order == null) return false;
            if (order.SellerCompanyId != companyId && order.BuyerCompanyId != companyId) return false;

            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ─── Private Helpers ──────────────────────────────────────────────────────

        private static OrderResponseDto MapToDto(Orders o, string buyerName = "") => new OrderResponseDto
        {
            Id = o.Id,
            BidId = o.BidId,
            SellerCompanyId = o.SellerCompanyId,
            SellerCompanyName = o.SellerCompany?.CompanyName ?? string.Empty,
            BuyerCompanyId = o.BuyerCompanyId,
            BuyerCompanyName = !string.IsNullOrEmpty(buyerName) ? buyerName : (o.BuyerCompany?.CompanyName ?? string.Empty),
            AgreedQuantity = o.AgreedQuantity,
            AgreedUnitPrice = o.AgreedUnitPrice,
            PlatformFee = o.PlatformFee,
            TotalAmount = o.TotalAmount,
            Status = o.Status,
            Notes = o.Notes,
            OrderDate = o.OrderDate
        };
    }
}
