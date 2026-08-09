using System;
using Mazaad.Domain.Enums;

namespace Mazaad.Application.DTOs.Payout
{
    /// <summary>
    /// Read-only response DTO for a SellerBankAccount.
    /// Returned by ISellerBankAccountService for display in company dashboards
    /// and the SuperAdmin verification panel.
    ///
    /// Sensitive fields (full account number, IBAN) are partially masked
    /// when returned to company users for security. SuperAdmin sees full values.
    /// The masking logic is applied in the service layer mapping, not here.
    /// </summary>
    public class SellerBankAccountDto
    {
        public int Id { get; set; }

        public int CompanyId { get; set; }

        /// <summary>Legal name of the account holder.</summary>
        public string AccountHolderName { get; set; } = string.Empty;

        /// <summary>Bank transfer or mobile wallet.</summary>
        public PayoutAccountType AccountType { get; set; }

        // ── Bank Transfer Fields ──────────────────────────────────────────────

        public string? BankName { get; set; }

        public string? BankCode { get; set; }

        /// <summary>
        /// Partially masked account number for security
        /// (e.g., "****1234"). The service layer applies the masking.
        /// </summary>
        public string? AccountNumber { get; set; }

        /// <summary>
        /// Partially masked IBAN for security
        /// (e.g., "EG38****3180002"). The service layer applies the masking.
        /// </summary>
        public string? Iban { get; set; }

        // ── Mobile Wallet Fields ──────────────────────────────────────────────

        /// <summary>
        /// Partially masked mobile wallet number
        /// (e.g., "0100***4567"). The service layer applies the masking.
        /// </summary>
        public string? MobileWalletNumber { get; set; }

        // ── Verification State ────────────────────────────────────────────────

        /// <summary>
        /// True if a SuperAdmin has manually verified this account.
        /// Payouts will only flow to verified accounts.
        /// </summary>
        public bool IsVerified { get; set; }

        /// <summary>ID of the SuperAdmin who verified this account. Null if not yet verified.</summary>
        public int? VerifiedByUserId { get; set; }

        /// <summary>Name of the SuperAdmin who verified this account. Null if not yet verified.</summary>
        public string? VerifiedByName { get; set; }

        /// <summary>When the account was verified. Null if not yet verified.</summary>
        public DateTime? VerifiedAt { get; set; }

        // ── Status Flags ──────────────────────────────────────────────────────

        /// <summary>
        /// True if this is the company's primary payout destination.
        /// PayoutService will target this account automatically on order delivery.
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>True if the account has been soft-deleted and is no longer active.</summary>
        public bool IsDeleted { get; set; }

        // ── Timestamps ────────────────────────────────────────────────────────

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
