using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mazaad.Application.DTOs.Escrow;
using Mazaad.Application.Interfaces.Services;
using Mazaad.Domain.Enums;
using Mazaad.Domain.Models;
using Mazaad.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Mazaad.Infrastructure.Services.Escrow
{
    public class EscrowService : IEscrowService
    {
        private readonly AppDbContext _context;
        private readonly IPayoutService _payoutService;
        private readonly IServiceProvider _serviceProvider;

        public EscrowService(
            AppDbContext context,
            IPayoutService payoutService,
            IServiceProvider serviceProvider)
        {
            _context = context;
            _payoutService = payoutService;
            _serviceProvider = serviceProvider;
        }

        public async Task<EscrowRecordDto> CreateEscrowAsync(int orderId, int sourcePaymentId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Check if escrow already exists for this order (idempotency check)
                var existingEscrow = await _context.EscrowRecords
                    .FirstOrDefaultAsync(e => e.OrderId == orderId);

                if (existingEscrow != null)
                {
                    // If it matches the same payment, return it. Otherwise raise exception.
                    if (existingEscrow.SourcePaymentId == sourcePaymentId)
                    {
                        await transaction.CommitAsync();
                        return MapToDto(existingEscrow);
                    }
                    throw new InvalidOperationException($"An EscrowRecord already exists for Order #{orderId} linked to a different payment.");
                }

                // Verify the order exists and load it
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                    throw new InvalidOperationException($"Order #{orderId} not found.");

                // Verify the payment is Paid
                var payment = await _context.Payments.FindAsync(sourcePaymentId);
                if (payment == null)
                    throw new InvalidOperationException($"Payment #{sourcePaymentId} not found.");

                if (payment.Status != PaymentStatus.Paid)
                    throw new InvalidOperationException($"Payment #{sourcePaymentId} is not in 'Paid' status (Current: {payment.Status}).");

                // Create the EscrowRecord
                var escrow = new EscrowRecord
                {
                    OrderId = order.Id,
                    SourcePaymentId = payment.Id,
                    AmountHeld = order.TotalAmount,
                    PlatformFee = order.PlatformFee,
                    SellerDueAmount = order.TotalAmount - order.PlatformFee,
                    Currency = payment.Currency,
                    Status = EscrowStatus.Held,
                    HeldAt = DateTime.UtcNow
                };

                _context.EscrowRecords.Add(escrow);
                await _context.SaveChangesAsync();

                // Link the EscrowRecordId back to the Payments row
                payment.EscrowRecordId = escrow.Id;
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return MapToDto(escrow);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task ReleaseEscrowAsync(int orderId)
        {
            var escrow = await _context.EscrowRecords
                .FirstOrDefaultAsync(e => e.OrderId == orderId && e.Status == EscrowStatus.Held);

            if (escrow == null)
                throw new InvalidOperationException($"No active 'Held' escrow found for Order #{orderId}.");

            // Initiate payout. PayoutService handles verified account validation.
            // If this throws, the escrow record is left as 'Held' and the transaction rolls back.
            await _payoutService.InitiatePayoutAsync(escrow.Id);
        }

        public async Task RefundEscrowAsync(int orderId, string reason)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var escrow = await _context.EscrowRecords
                    .FirstOrDefaultAsync(e => e.OrderId == orderId && e.Status == EscrowStatus.Held);

                if (escrow == null)
                    throw new InvalidOperationException($"No active 'Held' escrow found for Order #{orderId}.");

                // Resolve IPaymentService dynamically to break circular dependency
                var paymentService = _serviceProvider.GetRequiredService<IPaymentService>();

                var refundSuccess = await paymentService.InitiateRefundAsync(orderId, reason);
                if (!refundSuccess)
                    throw new InvalidOperationException("Failed to initiate refund transaction with Paymob API.");

                escrow.Status = EscrowStatus.Refunded;
                escrow.ReleasedAt = DateTime.UtcNow;
                escrow.Notes = string.IsNullOrWhiteSpace(escrow.Notes)
                    ? $"Refunded: {reason}"
                    : $"{escrow.Notes} | Refunded: {reason}";

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<EscrowRecordDto?> GetEscrowForOrderAsync(int orderId)
        {
            var escrow = await _context.EscrowRecords
                .FirstOrDefaultAsync(e => e.OrderId == orderId);

            return escrow == null ? null : MapToDto(escrow);
        }

        public async Task<IEnumerable<EscrowRecordDto>> GetAllEscrowsAsync(EscrowStatus? statusFilter = null)
        {
            var query = _context.EscrowRecords.AsQueryable();

            if (statusFilter.HasValue)
            {
                query = query.Where(e => e.Status == statusFilter.Value);
            }

            var escrows = await query
                .OrderByDescending(e => e.HeldAt)
                .ToListAsync();

            return escrows.Select(MapToDto);
        }

        // ── Helper ────────────────────────────────────────────────────────────

        private static EscrowRecordDto MapToDto(EscrowRecord e)
        {
            return new EscrowRecordDto
            {
                Id = e.Id,
                OrderId = e.OrderId,
                SourcePaymentId = e.SourcePaymentId,
                AmountHeld = e.AmountHeld,
                PlatformFee = e.PlatformFee,
                SellerDueAmount = e.SellerDueAmount,
                Currency = e.Currency,
                Status = e.Status,
                HeldAt = e.HeldAt,
                ReleasedAt = e.ReleasedAt,
                Notes = e.Notes
            };
        }
    }
}
