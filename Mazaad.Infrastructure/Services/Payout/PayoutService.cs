using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Application.Interfaces;
using Mazaad.Application.DTOs.Payout;
using Mazaad.Application.Interfaces.Services;
using Mazaad.Domain.Enums;
using Mazaad.Domain.Models;
using Mazaad.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mazaad.Infrastructure.Services.Payout
{
    public class PayoutService : IPayoutService
    {
        private readonly AppDbContext _context;
        private readonly PaymobDisbursementClient _disbursementClient;
        private readonly INotificationService _notificationService;
        private readonly ITelegramService _telegramService;

        public PayoutService(
            AppDbContext context,
            PaymobDisbursementClient disbursementClient,
            INotificationService notificationService,
            ITelegramService telegramService)
        {
            _context = context;
            _disbursementClient = disbursementClient;
            _notificationService = notificationService;
            _telegramService = telegramService;
        }

        public async Task<PayoutRecordDto> InitiatePayoutAsync(int escrowRecordId)
        {
            // 1. Load EscrowRecord, Order, and Seller details
            var escrow = await _context.EscrowRecords
                .Include(e => e.Order)
                .ThenInclude(o => o.SellerCompany)
                .FirstOrDefaultAsync(e => e.Id == escrowRecordId);

            if (escrow == null)
                throw new InvalidOperationException("Escrow record not found.");

            if (escrow.Status != EscrowStatus.Held)
                throw new InvalidOperationException("Escrow funds must be in 'Held' status to initiate a payout.");

            var sellerCompanyId = escrow.Order.SellerCompanyId;

            // 2. Load the seller company's default verified bank account
            var bankAccount = await _context.SellerBankAccounts
                .FirstOrDefaultAsync(a => a.CompanyId == sellerCompanyId && a.IsDefault && a.IsVerified && !a.IsDeleted);

            if (bankAccount == null)
            {
                var errorMsg = $"No default verified bank account or wallet registered for seller company '{escrow.Order.SellerCompany.CompanyName}' (ID: {sellerCompanyId}). Payout suspended.";
                
                // Alert the SuperAdmin via Telegram
                await _telegramService.SendReportAsync(
                    $"🚨 [Escrow & Payout System ALERT]\n{errorMsg}\nOrder ID: #{escrow.OrderId}\nEscrow ID: #{escrow.Id}\nPlease resolve manually.", 
                    null!);

                throw new InvalidOperationException(errorMsg);
            }

            // 3. Create a PayoutRecord with status Pending
            var payoutRecord = new PayoutRecord
            {
                EscrowRecordId = escrow.Id,
                SellerCompanyId = sellerCompanyId,
                SellerBankAccountId = bankAccount.Id,
                Amount = escrow.SellerDueAmount,
                Currency = escrow.Currency,
                Status = PayoutStatus.Pending,
                InitiatedAt = DateTime.UtcNow
            };

            _context.PayoutRecords.Add(payoutRecord);
            await _context.SaveChangesAsync();

            try
            {
                // 4. Authenticate and call Paymob's Disbursement API
                var authToken = await _disbursementClient.AuthenticateAsync();
                
                var amountCents = (long)Math.Round(escrow.SellerDueAmount * 100, MidpointRounding.AwayFromZero);

                var (paymobDisbursementId, paymobDisbursementRef) = await _disbursementClient.CreateDisbursementAsync(
                    authToken,
                    amountCents,
                    escrow.Currency,
                    bankAccount.AccountType,
                    bankAccount.AccountHolderName,
                    bankAccount.BankCode,
                    bankAccount.AccountNumber,
                    bankAccount.Iban,
                    bankAccount.MobileWalletNumber
                );

                // 5. Update PayoutRecord to Processing status
                payoutRecord.PaymobDisbursementId = paymobDisbursementId;
                payoutRecord.PaymobDisbursementRef = paymobDisbursementRef;
                payoutRecord.Status = PayoutStatus.Processing;

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Record the failure immediately in the DB but keep escrow HELD so we can retry later
                payoutRecord.Status = PayoutStatus.Failed;
                payoutRecord.FailureReason = ex.Message;
                payoutRecord.CompletedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                await _telegramService.SendReportAsync(
                    $"🚨 [Paymob Disbursement API Error]\nFailed to submit disbursement for Order #{escrow.OrderId}.\nError: {ex.Message}\nEscrow ID: #{escrow.Id}",
                    null!);

                throw;
            }

            return MapToDto(payoutRecord);
        }

        public async Task<bool> HandlePayoutWebhookAsync(JsonElement payload, string signatureFromHeader)
        {
            if (!payload.TryGetProperty("obj", out var obj))
                return false;

            // Verify webhook signature
            if (!_disbursementClient.VerifyWebhookSignature(obj, signatureFromHeader))
                return false;

            var success = obj.GetProperty("success").GetBoolean();
            var paymobDisbursementId = obj.GetProperty("id").GetRawText();
            var transactionRef = obj.TryGetProperty("transaction_reference", out var refProp) 
                ? refProp.GetString() 
                : null;

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Find matching payout record
                var payout = await _context.PayoutRecords
                    .Include(p => p.EscrowRecord)
                    .ThenInclude(e => e.Order)
                    .FirstOrDefaultAsync(p => p.PaymobDisbursementId == paymobDisbursementId);

                if (payout == null) return false;

                // Ignore if already processed (idempotency guard)
                if (payout.Status == PayoutStatus.Completed || payout.Status == PayoutStatus.Failed)
                    return true;

                payout.CompletedAt = DateTime.UtcNow;

                if (success)
                {
                    // Update Payout
                    payout.Status = PayoutStatus.Completed;
                    payout.PaymobDisbursementRef = transactionRef ?? payout.PaymobDisbursementRef;

                    // Update Escrow status
                    payout.EscrowRecord.Status = EscrowStatus.Released;
                    payout.EscrowRecord.ReleasedAt = DateTime.UtcNow;

                    // Update Order status to Completed
                    payout.EscrowRecord.Order.Status = OrderStatus.Completed;
                    payout.EscrowRecord.Order.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Send notification to seller users
                    var sellerUsers = await _context.Users
                        .Where(u => u.CompanyId == payout.SellerCompanyId)
                        .ToListAsync();

                    foreach (var user in sellerUsers)
                    {
                        await _notificationService.CreateNotificationAsync(
                            user.Id,
                            "Payout Completed",
                            $"Disbursement of {payout.Amount} {payout.Currency} for Order #{payout.EscrowRecord.OrderId} has been transferred to your account successfully.",
                            "Order",
                            payout.EscrowRecord.OrderId
                        );
                    }
                }
                else
                {
                    // Update Payout status
                    payout.Status = PayoutStatus.Failed;
                    payout.FailureReason = obj.TryGetProperty("failure_reason", out var fail) ? fail.GetString() : "Paymob transaction rejected";

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Alert Admin for manual review
                    await _telegramService.SendReportAsync(
                        $"🚨 [Payout Webhook Failure]\nPaymob reported disbursement failed for Payout ID: {payout.Id}.\nReason: {payout.FailureReason}\nOrder ID: #{payout.EscrowRecord.OrderId}",
                        null!);
                }

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                
                await _telegramService.SendReportAsync(
                    $"🚨 [Payout Webhook Error]\nFailed to process webhook for disbursement ID {paymobDisbursementId}.\nError: {ex.Message}",
                    null!);
                
                return false;
            }
        }

        public async Task<PayoutRecordDto> RetryPayoutAsync(int payoutRecordId)
        {
            var failedPayout = await _context.PayoutRecords
                .Include(p => p.EscrowRecord)
                .FirstOrDefaultAsync(p => p.Id == payoutRecordId);

            if (failedPayout == null)
                throw new InvalidOperationException("Payout record not found.");

            if (failedPayout.Status != PayoutStatus.Failed)
                throw new InvalidOperationException("Only failed payouts can be retried.");

            if (failedPayout.EscrowRecord.Status != EscrowStatus.Held)
                throw new InvalidOperationException("Payout retry is blocked because escrow status is not 'Held'.");

            // Initiate a brand new PayoutRecord to maintain immutable audit trace
            return await InitiatePayoutAsync(failedPayout.EscrowRecordId);
        }

        public async Task<IEnumerable<PayoutRecordDto>> GetPayoutsForSellerAsync(int sellerCompanyId)
        {
            var payouts = await _context.PayoutRecords
                .Include(p => p.EscrowRecord)
                .Include(p => p.SellerCompany)
                .Include(p => p.DestinationAccount)
                .Where(p => p.SellerCompanyId == sellerCompanyId)
                .OrderByDescending(p => p.InitiatedAt)
                .ToListAsync();

            return payouts.Select(MapToDto);
        }

        public async Task<IEnumerable<PayoutRecordDto>> GetAllPayoutsAsync(PayoutStatus? statusFilter = null)
        {
            var query = _context.PayoutRecords
                .Include(p => p.EscrowRecord)
                .Include(p => p.SellerCompany)
                .Include(p => p.DestinationAccount)
                .AsQueryable();

            if (statusFilter.HasValue)
            {
                query = query.Where(p => p.Status == statusFilter.Value);
            }

            var payouts = await query
                .OrderByDescending(p => p.InitiatedAt)
                .ToListAsync();

            return payouts.Select(MapToDto);
        }

        public async Task<PayoutRecordDto?> GetPayoutByIdAsync(int payoutRecordId)
        {
            var payout = await _context.PayoutRecords
                .Include(p => p.EscrowRecord)
                .Include(p => p.SellerCompany)
                .Include(p => p.DestinationAccount)
                .FirstOrDefaultAsync(p => p.Id == payoutRecordId);

            return payout == null ? null : MapToDto(payout);
        }

        // ── Helper ────────────────────────────────────────────────────────────

        private static PayoutRecordDto MapToDto(PayoutRecord p)
        {
            var summary = p.DestinationAccount.AccountType == PayoutAccountType.BankTransfer
                ? $"{p.DestinationAccount.BankName} (****{p.DestinationAccount.AccountNumber?.Substring(Math.Max(0, (p.DestinationAccount.AccountNumber?.Length ?? 0) - 4))})"
                : $"Wallet ({p.DestinationAccount.MobileWalletNumber?.Substring(0, 4)}***{p.DestinationAccount.MobileWalletNumber?.Substring(Math.Max(0, (p.DestinationAccount.MobileWalletNumber?.Length ?? 0) - 4))})";

            return new PayoutRecordDto
            {
                Id = p.Id,
                EscrowRecordId = p.EscrowRecordId,
                OrderId = p.EscrowRecord.OrderId,
                SellerCompanyId = p.SellerCompanyId,
                SellerCompanyName = p.SellerCompany?.CompanyName ?? string.Empty,
                SellerBankAccountId = p.SellerBankAccountId,
                DestinationAccountSummary = summary,
                Amount = p.Amount,
                Currency = p.Currency,
                Status = p.Status,
                PaymobDisbursementId = p.PaymobDisbursementId,
                PaymobDisbursementRef = p.PaymobDisbursementRef,
                FailureReason = p.FailureReason,
                InitiatedAt = p.InitiatedAt,
                CompletedAt = p.CompletedAt
            };
        }
    }
}
