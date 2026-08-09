using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Mazaad.Domain.Enums;

namespace Mazaad.Domain.Models
{
    /// <summary>
    /// Stores a verified payout destination (bank account or mobile wallet)
    /// belonging to a seller company.
    ///
    /// Verification Rule: IsVerified must be true before ANY payout can be
    /// disbursed to this account. A SuperAdmin must manually verify the account
    /// details against the company's registration documents.
    ///
    /// Default Rule: At most one account per company can have IsDefault = true.
    /// This is enforced by a unique filtered index in AppDbContext.OnModelCreating:
    ///   HasIndex(a => a.CompanyId).HasFilter("[IsDefault] = 1").IsUnique()
    ///
    /// Soft Delete: Accounts are never hard-deleted. IsDeleted = true prevents
    /// new payouts from using the account while preserving historical PayoutRecord
    /// references (which use DeleteBehavior.NoAction).
    /// </summary>
    public class SellerBankAccount
    {
        [Key]
        public int Id { get; set; }

        // ── Owner ─────────────────────────────────────────────────────────────────

        [ForeignKey(nameof(Company))]
        public int CompanyId { get; set; }

        // ── Identity ──────────────────────────────────────────────────────────────

        /// <summary>
        /// The legal name of the account holder. Must match the company's
        /// registered name exactly as it appears in bank records.
        /// </summary>
        [Required, MaxLength(200)]
        public string AccountHolderName { get; set; } = string.Empty;

        public PayoutAccountType AccountType { get; set; }

        // ── Bank Account Fields (used when AccountType = BankTransfer) ────────────

        /// <summary>
        /// Human-readable bank name (e.g., "National Bank of Egypt", "CIB").
        /// Required when AccountType = BankTransfer.
        /// </summary>
        [MaxLength(150)]
        public string? BankName { get; set; }

        /// <summary>
        /// Paymob's internal bank code for the recipient bank.
        /// Required when AccountType = BankTransfer. Obtained from Paymob's
        /// list of supported banks in their Disbursement API documentation.
        /// </summary>
        [MaxLength(20)]
        public string? BankCode { get; set; }

        /// <summary>
        /// The bank account number (without spaces or dashes).
        /// Required when AccountType = BankTransfer.
        /// </summary>
        [MaxLength(50)]
        public string? AccountNumber { get; set; }

        /// <summary>
        /// International Bank Account Number (e.g., EG380019000500000000263180002).
        /// Optional but recommended for interbank transfers to reduce rejection risk.
        /// </summary>
        [MaxLength(34)]
        public string? Iban { get; set; }

        // ── Mobile Wallet Fields (used when AccountType = MobileWallet) ───────────

        /// <summary>
        /// The Egyptian mobile wallet number (11 digits, starting with 01).
        /// Required when AccountType = MobileWallet.
        /// The wallet provider is inferred from the number prefix by Paymob
        /// (01x = Vodafone, 01y = Orange, etc.).
        /// </summary>
        [MaxLength(11)]
        public string? MobileWalletNumber { get; set; }

        // ── Verification ──────────────────────────────────────────────────────────

        /// <summary>
        /// True only after a SuperAdmin has manually confirmed that these banking
        /// details are legitimate and match the company's legal documents.
        /// No payout will flow to an account where IsVerified = false.
        /// </summary>
        public bool IsVerified { get; set; } = false;

        /// <summary>
        /// The SuperAdmin who verified this account. Null until IsVerified = true.
        /// </summary>
        [ForeignKey(nameof(VerifiedBy))]
        public int? VerifiedByUserId { get; set; }

        /// <summary>When the SuperAdmin set IsVerified = true.</summary>
        public DateTime? VerifiedAt { get; set; }

        // ── Default Flag ──────────────────────────────────────────────────────────

        /// <summary>
        /// Whether this is the company's primary payout destination.
        /// PayoutService always selects the account where IsDefault = true
        /// and IsVerified = true. At most one account per company can be true.
        /// Enforced by a unique filtered index in AppDbContext.
        /// </summary>
        public bool IsDefault { get; set; } = false;

        // ── Soft Delete ───────────────────────────────────────────────────────────

        /// <summary>
        /// Soft delete flag. Deleted accounts are excluded from payout selection
        /// but are preserved for historical PayoutRecord references.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        // ── Timestamps ────────────────────────────────────────────────────────────

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ── Navigation ────────────────────────────────────────────────────────────

        public Companies Company { get; set; } = null!;

        /// <summary>The SuperAdmin who verified this account. Null if not yet verified.</summary>
        public ApplicationUser? VerifiedBy { get; set; }

        /// <summary>
        /// All payout attempts that used this account as the destination.
        /// Preserved even if the account is soft-deleted (DeleteBehavior.NoAction
        /// is configured in AppDbContext to prevent cascade-deleting PayoutRecords).
        /// </summary>
        public ICollection<PayoutRecord> Payouts { get; set; } = new HashSet<PayoutRecord>();
    }
}
