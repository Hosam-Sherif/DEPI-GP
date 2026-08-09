using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces;
using Mazaad.Application.DTOs;
using Mazaad.Application.DTOs.Escrow;
using Mazaad.Application.Interfaces.Services;
using Mazaad.Domain.Enums;
using Mazaad.Domain.Models;
using Mazaad.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mazaad.Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IEscrowService _escrowService;
        private readonly ITelegramService _telegramService;

        public OrderService(
            AppDbContext context,
            INotificationService notificationService,
            IEscrowService escrowService,
            ITelegramService telegramService)
        {
            _context = context;
            _notificationService = notificationService;
            _escrowService = escrowService;
            _telegramService = telegramService;
        }


        // ─── List Orders ──────────────────────────────────────────────────────────

        public async Task<IEnumerable<OrderResponseDto>> GetOrdersForCompanyAsync(int companyId)
        {
            var orders = await _context.Orders
                .Include(o => o.SellerCompany)
                .Include(o => o.BuyerCompany)
                .Include(o => o.Bid)
                .Include(o => o.Escrow)
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
                .Include(o => o.Escrow)
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

            // 🔴 تعديل: شرط جديد بالكامل — الحماية دي ضرورية عشان Order.BuyerCompanyId لسه إجباري
            if (bid.BuyerCompanyId == null)
                throw new InvalidOperationException(
                    "This bid was placed by an individual bidder. Order/payment fulfillment for individual buyers is not supported yet.");

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
                BuyerCompanyId = bid.BuyerCompanyId.Value,   // 🔴 تعديل: كانت bid.BuyerCompanyId مباشرة (بقت int? فمحتاجة .Value بعد التأكد إنها مش null)
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
            var order = await _context.Orders
                .Include(o => o.SellerCompany)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return false;
            if (order.SellerCompanyId != companyId && order.BuyerCompanyId != companyId) return false;

            if (order.Status == OrderStatus.Completed && newStatus != OrderStatus.Completed)
                throw new InvalidOperationException("Cannot change status of a completed order.");

            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Escrow side-effects based on new status
            if (newStatus == OrderStatus.Delivered)
            {
                try
                {
                    // Automatic disbursement of held funds to the seller
                    await _escrowService.ReleaseEscrowAsync(order.Id);
                }
                catch (Exception ex)
                {
                    // Send alert but keep the order status as Delivered (physical delivery holds)
                    await _telegramService.SendReportAsync(
                        $"🚨 [Escrow Payout Initiation Failed]\nDelivery confirmed for Order #{order.Id}, but automated seller payout could not be initiated.\nError: {ex.Message}\nSeller Company: {order.SellerCompany.CompanyName}",
                        null!);
                }
            }
            else if (newStatus == OrderStatus.Cancelled)
            {
                var escrow = await _context.EscrowRecords
                    .FirstOrDefaultAsync(e => e.OrderId == order.Id);

                if (escrow != null && escrow.Status == EscrowStatus.Held)
                {
                    try
                    {
                        // Automatic refund to the buyer
                        await _escrowService.RefundEscrowAsync(order.Id, "Order cancelled by participant.");
                    }
                    catch (Exception ex)
                    {
                        await _telegramService.SendReportAsync(
                            $"🚨 [Escrow Refund Failed]\nOrder #{order.Id} was cancelled, but buyer refund failed.\nError: {ex.Message}",
                            null!);
                    }
                }
            }

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
            OrderDate = o.OrderDate,
            Escrow = o.Escrow == null ? null : new EscrowStatusSummaryDto
            {
                Status = o.Escrow.Status,
                HeldAt = o.Escrow.HeldAt,
                ReleasedAt = o.Escrow.ReleasedAt,
                SellerDueAmount = o.Escrow.SellerDueAmount,
                Currency = o.Escrow.Currency
            }
        };
    }

}