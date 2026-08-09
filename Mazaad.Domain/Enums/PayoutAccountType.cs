namespace Mazaad.Domain.Enums
{
    /// <summary>
    /// Distinguishes the destination type of a SellerBankAccount.
    /// Paymob's Disbursement API uses different request payloads and
    /// integration IDs depending on the payout destination type.
    /// </summary>
    public enum PayoutAccountType
    {
        /// <summary>
        /// A standard Egyptian bank account. Requires BankCode, AccountNumber,
        /// and optionally an IBAN. Paymob routes the transfer through the
        /// interbank EFT network.
        /// </summary>
        BankTransfer = 0,

        /// <summary>
        /// An Egyptian mobile wallet (e.g., Vodafone Cash, Orange Money,
        /// Etisalat Cash, WePay). Requires a valid 11-digit Egyptian mobile
        /// number registered with the wallet provider.
        /// </summary>
        MobileWallet = 1
    }
}
