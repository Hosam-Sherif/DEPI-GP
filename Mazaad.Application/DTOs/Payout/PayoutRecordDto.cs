using System;
using Mazaad.Domain.Enums;

namespace Mazaad.Application.DTOs.Payout
{
    /// <summary>
    /// Read-only response DTO for a PayoutRecord — a single Paymob disbursement attempt.
    /// Returned by IPayoutService and exposed via PayoutsController.
    ///
    /// Immutability note: once a PayoutRecord reaches a terminal state (Completed,
    /// Failed, Cancelled), the record is never changed. This DTO reflects the
    /// final state of a specific attempt for auditing.
    /// </summary>
    public class PayoutRecordDto
    {
        public int Id { get; set; }

        // ── Parent Escrow ─────────────────────────────────────────────────────

        /// <summary>The EscrowRecord that owns the funds being disbursed.</summary>
        public int EscrowRecordId { get; set; }

        /// <summary>The Order whose funds are being disbursed (via EscrowRecord).</summary>
        public int OrderId { get; set; }

        // ── Seller ────────────────────────────────────────────────────────────

        public int SellerCompanyId { get; set; }
        public string SellerCompanyName { get; set; } = string.Empty;

        // ── Destination Account ───────────────────────────────────────────────

        /// <summary>ID of the SellerBankAccount used for this attempt.</summary>
        public int SellerBankAccountId { get; set; }

        /// <summary>
        /// Masked account identifier for display (e.g., "CIB ****1234"
        /// or "Vodafone Cash 0100***4567"). Built by the service layer.
        /// </summary>
        public string DestinationAccountSummary { get; set; } = string.Empty;

        // ── Financial ─────────────────────────────────────────────────────────

        /// <summary>Amount disbursed in this attempt (SellerDueAmount from EscrowRecord).</summary>
        public decimal Amount { get; set; }

        public string Currency { get; set; } = "EGP";

        // ── State ─────────────────────────────────────────────────────────────

        public PayoutStatus Status { get; set; }

        /// <summary>Human-readable status label for frontend display.</summary>
        public string StatusLabel => Status switch
        {
            PayoutStatus.Pending    => "Queued",
            PayoutStatus.Processing => "Transfer In Progress",
            PayoutStatus.Completed  => "Transfer Completed",
            PayoutStatus.Failed     => "Transfer Failed",
            PayoutStatus.Cancelled  => "Cancelled",
            _                       => Status.ToString()
        };

        // ── Paymob Tracking ───────────────────────────────────────────────────

        /// <summary>
        /// Paymob's disbursement ID for this transaction.
        /// Null while Status = Pending (API call not yet made).
        /// </summary>
        public string? PaymobDisbursementId { get; set; }

        /// <summary>Paymob's human-readable reference number for the bank transfer.</summary>
        public string? PaymobDisbursementRef { get; set; }

        // ── Failure Detail ────────────────────────────────────────────────────

        /// <summary>
        /// Populated when Status = Failed. Contains the rejection reason
        /// from Paymob (e.g., "Invalid IBAN", "Account closed by bank").
        /// </summary>
        public string? FailureReason { get; set; }

        // ── Timestamps ────────────────────────────────────────────────────────

        /// <summary>When the Paymob Disbursement API was called for this attempt.</summary>
        public DateTime InitiatedAt { get; set; }

        /// <summary>
        /// When Paymob confirmed the result (success or failure) via webhook.
        /// Null while Status is Pending or Processing.
        /// </summary>
        public DateTime? CompletedAt { get; set; }
    }
}
