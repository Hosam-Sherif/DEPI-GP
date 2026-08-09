using System.Collections.Generic;
using System.Threading.Tasks;
using Mazaad.Application.DTOs.Payout;

namespace Mazaad.Application.Interfaces.Services
{
    /// <summary>
    /// Manages SellerBankAccount entities — the registered payout destinations
    /// (bank accounts and mobile wallets) for seller companies.
    ///
    /// VERIFICATION RULE: IPayoutService will ONLY disburse funds to an account
    /// where IsVerified = true AND IsDefault = true AND IsDeleted = false.
    /// SuperAdmin verification is a mandatory gating step, not optional.
    /// </summary>
    public interface ISellerBankAccountService
    {
        /// <summary>
        /// Registers a new bank account or mobile wallet for a seller company.
        ///
        /// POSTCONDITIONS:
        ///   - Account is created with IsVerified = false and IsDeleted = false.
        ///   - If this is the company's first account, IsDefault is set to true
        ///     automatically (a company with only one account should have it as default).
        ///   - If this is NOT the first account, IsDefault = false; the company must
        ///     explicitly call SetDefaultAccountAsync to change the default.
        /// </summary>
        /// <param name="companyId">The seller company registering the account.</param>
        /// <param name="dto">The account details.</param>
        /// <returns>The newly created SellerBankAccountDto.</returns>
        Task<SellerBankAccountDto> AddBankAccountAsync(int companyId, CreateSellerBankAccountDto dto);

        /// <summary>
        /// Returns all bank accounts registered by a company, including soft-deleted ones
        /// for SuperAdmin, but excluding soft-deleted ones for company users.
        /// </summary>
        /// <param name="companyId">The seller company whose accounts to retrieve.</param>
        /// <param name="includeDeleted">If true, includes soft-deleted accounts. SuperAdmin only.</param>
        Task<IEnumerable<SellerBankAccountDto>> GetAccountsForCompanyAsync(
            int companyId, bool includeDeleted = false);

        /// <summary>
        /// Returns the detail of a single SellerBankAccount.
        /// Returns null if not found or if the account does not belong to companyId.
        /// </summary>
        Task<SellerBankAccountDto?> GetAccountByIdAsync(int accountId, int companyId);

        /// <summary>
        /// Atomically sets one account as IsDefault = true and clears IsDefault = false
        /// on all other accounts belonging to the same company.
        ///
        /// Uses a database transaction to prevent race conditions where two accounts
        /// could simultaneously have IsDefault = true.
        ///
        /// PRECONDITIONS:
        ///   - The account must belong to companyId.
        ///   - The account must not be soft-deleted (IsDeleted = false).
        /// </summary>
        Task<SellerBankAccountDto> SetDefaultAccountAsync(int companyId, int accountId);

        /// <summary>
        /// SuperAdmin marks a SellerBankAccount as verified after manually confirming
        /// the bank details against the company's registration documents.
        ///
        /// POSTCONDITIONS:
        ///   - IsVerified = true, VerifiedByUserId = adminUserId, VerifiedAt = now.
        ///   - The account becomes eligible for payouts (if IsDefault = true).
        /// </summary>
        /// <param name="accountId">The account to verify.</param>
        /// <param name="adminUserId">The SuperAdmin performing the verification.</param>
        Task<SellerBankAccountDto> VerifyAccountAsync(int accountId, int adminUserId);

        /// <summary>
        /// Soft-deletes a SellerBankAccount by setting IsDeleted = true.
        ///
        /// PRECONDITIONS (if any fail, throws InvalidOperationException):
        ///   - The account must belong to companyId.
        ///   - No PayoutRecord with Status = Pending or Processing may reference this account.
        ///     (A payout that is mid-flight cannot be cancelled by removing the account.)
        ///   - If the account being deleted is IsDefault = true, the caller must first
        ///     designate a different account as default via SetDefaultAccountAsync.
        ///     (Prevents the company from being left with no default payout destination.)
        /// </summary>
        Task<bool> DeleteAccountAsync(int companyId, int accountId);
    }
}
