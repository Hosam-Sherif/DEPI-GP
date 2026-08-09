using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mazaad.Application.DTOs.Payout;
using Mazaad.Application.Interfaces.Services;
using Mazaad.Domain.Models;
using Mazaad.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mazaad.Infrastructure.Services.Payout
{
    public class SellerBankAccountService : ISellerBankAccountService
    {
        private readonly AppDbContext _context;

        public SellerBankAccountService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SellerBankAccountDto> AddBankAccountAsync(int companyId, CreateSellerBankAccountDto dto)
        {
            // Validate request data based on AccountType
            if (dto.AccountType == Domain.Enums.PayoutAccountType.BankTransfer)
            {
                if (string.IsNullOrWhiteSpace(dto.BankName))
                    throw new InvalidOperationException("Bank Name is required for Bank Transfer payout method.");
                if (string.IsNullOrWhiteSpace(dto.BankCode))
                    throw new InvalidOperationException("Bank Code is required for Bank Transfer payout method.");
                if (string.IsNullOrWhiteSpace(dto.AccountNumber))
                    throw new InvalidOperationException("Account Number is required for Bank Transfer payout method.");
            }
            else // Mobile Wallet
            {
                if (string.IsNullOrWhiteSpace(dto.MobileWalletNumber))
                    throw new InvalidOperationException("Mobile Wallet Number is required for Mobile Wallet payout method.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Check if this company has any existing bank accounts (non-deleted)
                var hasAnyAccounts = await _context.SellerBankAccounts
                    .AnyAsync(a => a.CompanyId == companyId && !a.IsDeleted);

                var account = new SellerBankAccount
                {
                    CompanyId = companyId,
                    AccountHolderName = dto.AccountHolderName,
                    AccountType = dto.AccountType,
                    BankName = dto.AccountType == Domain.Enums.PayoutAccountType.BankTransfer ? dto.BankName : null,
                    BankCode = dto.AccountType == Domain.Enums.PayoutAccountType.BankTransfer ? dto.BankCode : null,
                    AccountNumber = dto.AccountType == Domain.Enums.PayoutAccountType.BankTransfer ? dto.AccountNumber : null,
                    Iban = dto.AccountType == Domain.Enums.PayoutAccountType.BankTransfer ? dto.Iban : null,
                    MobileWalletNumber = dto.AccountType == Domain.Enums.PayoutAccountType.MobileWallet ? dto.MobileWalletNumber : null,
                    IsVerified = false,
                    IsDeleted = false,
                    // If first account, set it as default automatically
                    IsDefault = !hasAnyAccounts,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.SellerBankAccounts.Add(account);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return MapToDto(account, mask: false);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<SellerBankAccountDto>> GetAccountsForCompanyAsync(
            int companyId, bool includeDeleted = false)
        {
            var query = _context.SellerBankAccounts
                .Include(a => a.VerifiedBy)
                .Where(a => a.CompanyId == companyId);

            if (!includeDeleted)
            {
                query = query.Where(a => !a.IsDeleted);
            }

            var accounts = await query
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.CreatedAt)
                .ToListAsync();

            // Mask details for normal listing view
            return accounts.Select(a => MapToDto(a, mask: true));
        }

        public async Task<SellerBankAccountDto?> GetAccountByIdAsync(int accountId, int companyId)
        {
            var account = await _context.SellerBankAccounts
                .Include(a => a.VerifiedBy)
                .FirstOrDefaultAsync(a => a.Id == accountId && a.CompanyId == companyId && !a.IsDeleted);

            return account == null ? null : MapToDto(account, mask: false);
        }

        public async Task<SellerBankAccountDto> SetDefaultAccountAsync(int companyId, int accountId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var targetAccount = await _context.SellerBankAccounts
                    .FirstOrDefaultAsync(a => a.Id == accountId && a.CompanyId == companyId && !a.IsDeleted);

                if (targetAccount == null)
                    throw new InvalidOperationException("Bank account not found or has been deleted.");

                // Load all other default accounts for this company to turn them off
                var defaultAccounts = await _context.SellerBankAccounts
                    .Where(a => a.CompanyId == companyId && a.IsDefault && a.Id != accountId)
                    .ToListAsync();

                foreach (var acc in defaultAccounts)
                {
                    acc.IsDefault = false;
                    acc.UpdatedAt = DateTime.UtcNow;
                }

                targetAccount.IsDefault = true;
                targetAccount.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return MapToDto(targetAccount, mask: false);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<SellerBankAccountDto> VerifyAccountAsync(int accountId, int adminUserId)
        {
            var account = await _context.SellerBankAccounts
                .FirstOrDefaultAsync(a => a.Id == accountId && !a.IsDeleted);

            if (account == null)
                throw new InvalidOperationException("Bank account not found or has been deleted.");

            account.IsVerified = true;
            account.VerifiedByUserId = adminUserId;
            account.VerifiedAt = DateTime.UtcNow;
            account.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Load the admin user's name for mapping
            await _context.Entry(account).Reference(a => a.VerifiedBy).LoadAsync();

            return MapToDto(account, mask: false);
        }

        public async Task<bool> DeleteAccountAsync(int companyId, int accountId)
        {
            var account = await _context.SellerBankAccounts
                .FirstOrDefaultAsync(a => a.Id == accountId && a.CompanyId == companyId && !a.IsDeleted);

            if (account == null)
                return false;

            // Enforce Rule: cannot delete if active payout is referencing it
            var hasInFlightPayout = await _context.PayoutRecords
                .AnyAsync(p => p.SellerBankAccountId == accountId && 
                              (p.Status == Domain.Enums.PayoutStatus.Pending || 
                               p.Status == Domain.Enums.PayoutStatus.Processing));

            if (hasInFlightPayout)
                throw new InvalidOperationException(
                    "Cannot delete this bank account because there is a payout transfer currently in progress to it.");

            // Enforce Rule: cannot delete default account unless another default is set first
            if (account.IsDefault)
            {
                var hasOtherAccount = await _context.SellerBankAccounts
                    .AnyAsync(a => a.CompanyId == companyId && a.Id != accountId && !a.IsDeleted);

                if (hasOtherAccount)
                {
                    throw new InvalidOperationException(
                        "Cannot delete the default payout destination. Please select and set another account as default first.");
                }
            }

            account.IsDeleted = true;
            account.IsDefault = false;
            account.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ── Helper ────────────────────────────────────────────────────────────

        private static SellerBankAccountDto MapToDto(SellerBankAccount a, bool mask)
        {
            return new SellerBankAccountDto
            {
                Id = a.Id,
                CompanyId = a.CompanyId,
                AccountHolderName = a.AccountHolderName,
                AccountType = a.AccountType,
                BankName = a.BankName,
                BankCode = a.BankCode,
                AccountNumber = mask ? MaskValue(a.AccountNumber, 4) : a.AccountNumber,
                Iban = mask ? MaskValue(a.Iban, 8) : a.Iban,
                MobileWalletNumber = mask ? MaskValue(a.MobileWalletNumber, 4) : a.MobileWalletNumber,
                IsVerified = a.IsVerified,
                VerifiedByUserId = a.VerifiedByUserId,
                VerifiedByName = a.VerifiedBy?.FullName,
                VerifiedAt = a.VerifiedAt,
                IsDefault = a.IsDefault,
                IsDeleted = a.IsDeleted,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            };
        }

        private static string? MaskValue(string? value, int keepEndLength)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;
            if (value.Length <= keepEndLength) return new string('*', value.Length);

            var maskLength = value.Length - keepEndLength;
            return new string('*', maskLength) + value.Substring(maskLength);
        }
    }
}
