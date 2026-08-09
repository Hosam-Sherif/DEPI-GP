using System.ComponentModel.DataAnnotations;
using Mazaad.Domain.Enums;

namespace Mazaad.Application.DTOs.Payout
{
    /// <summary>
    /// Request DTO used when a seller company registers a new bank account or mobile wallet
    /// as a payout destination. Submitted to ISellerBankAccountService.AddBankAccountAsync.
    ///
    /// VALIDATION RULES (enforced in the service layer, not just annotations):
    ///   BankTransfer accounts: BankName, BankCode, and AccountNumber are required.
    ///   MobileWallet accounts: MobileWalletNumber is required; bank fields are ignored.
    /// </summary>
    public class CreateSellerBankAccountDto
    {
        /// <summary>
        /// The legal name of the account holder. Must match the company's registered
        /// name exactly as it appears in official bank records. Used verbatim in
        /// Paymob's disbursement request payload.
        /// </summary>
        [Required(ErrorMessage = "Account holder name is required.")]
        [MaxLength(200, ErrorMessage = "Account holder name must not exceed 200 characters.")]
        public string AccountHolderName { get; set; } = string.Empty;

        /// <summary>
        /// The destination type. Determines which fields are required and which
        /// Paymob disbursement product is used.
        /// </summary>
        [Required]
        public PayoutAccountType AccountType { get; set; }

        // ── Bank Transfer Fields ──────────────────────────────────────────────

        /// <summary>
        /// Human-readable bank name (e.g., "National Bank of Egypt", "CIB", "Banque Misr").
        /// Required when AccountType = BankTransfer.
        /// </summary>
        [MaxLength(150, ErrorMessage = "Bank name must not exceed 150 characters.")]
        public string? BankName { get; set; }

        /// <summary>
        /// Paymob's internal bank code for the recipient institution.
        /// Obtain the list of supported codes from Paymob's Disbursement API documentation.
        /// Required when AccountType = BankTransfer.
        /// </summary>
        [MaxLength(20, ErrorMessage = "Bank code must not exceed 20 characters.")]
        public string? BankCode { get; set; }

        /// <summary>
        /// The bank account number without spaces or dashes.
        /// Required when AccountType = BankTransfer.
        /// </summary>
        [MaxLength(50, ErrorMessage = "Account number must not exceed 50 characters.")]
        public string? AccountNumber { get; set; }

        /// <summary>
        /// International Bank Account Number (e.g., EG380019000500000000263180002).
        /// Optional but strongly recommended to reduce bank rejection rates.
        /// Maximum 34 characters per the IBAN standard.
        /// </summary>
        [MaxLength(34, ErrorMessage = "IBAN must not exceed 34 characters.")]
        public string? Iban { get; set; }

        // ── Mobile Wallet Fields ──────────────────────────────────────────────

        /// <summary>
        /// Egyptian mobile wallet number. Must be exactly 11 digits starting with 01.
        /// The wallet provider (Vodafone, Orange, Etisalat, WePay) is determined by
        /// Paymob based on the number prefix.
        /// Required when AccountType = MobileWallet.
        /// </summary>
        [MaxLength(11, ErrorMessage = "Mobile wallet number must be exactly 11 digits.")]
        [RegularExpression(@"^01[0125]\d{8}$",
            ErrorMessage = "Mobile wallet number must be a valid Egyptian mobile number starting with 01.")]
        public string? MobileWalletNumber { get; set; }
    }
}
