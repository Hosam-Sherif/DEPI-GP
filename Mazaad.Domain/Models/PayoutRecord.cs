using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Mazaad.Domain.Enums;

namespace Mazaad.Domain.Models
{
    /// <summary>
    /// Represents a single Paymob Disbursement attempt to transfer the seller's
    /// due amount from the platform's Paymob merchant account to the seller's
    /// registered bank account or mobile wallet.
    ///
    /// Immutability Rule: A PayoutRecord is NEVER mutated after it reaches a
    /// terminal state (Completed, Failed, Cancelled). If a payout fails and must
    /// be retried, a NEW PayoutRecord is created, referencing the same EscrowRecord.
    /// This preserves a complete, tamper-proof audit trail of every disbursement attempt.
    /// </summary>
    public class PayoutRecord
    {
        [Key]
        public int Id { get; set; }

        // ── Parent Escrow ─────────────────────────────────────────────────────────

        /// <summary>
        /// The EscrowRecord that owns the funds being disbursed.
        /// Many-to-one: one escrow can have multiple payout attempts (on retry).
        /// </summary>
        [ForeignKey(nameof(EscrowRecord))]
        public int EscrowRecordId { get; set; }

        // ── Seller Identifiers ────────────────────────────────────────────────────

        /// <summary>
        /// Denormalized FK to the seller company. Avoids a join through EscrowRecord
        /// → Order → SellerCompanyId when querying a seller's payout history.
        /// </summary>
        [ForeignKey(nameof(SellerCompany))]
        public int SellerCompanyId { get; set; }

        /// <summary>
        /// The specific bank account or mobile wallet used for this disbursement attempt.
        /// Snapshotted at initiation time. If the account is later deleted, this FK
        /// is preserved (DeleteBehavior.NoAction) so the audit trail remains intact.
        /// </summary>
        [ForeignKey(nameof(DestinationAccount))]
        public int SellerBankAccountId { get; set; }

        // ── Financial ─────────────────────────────────────────────────────────────

        /// <summary>
        /// The amount disbursed in this attempt. Equals EscrowRecord.SellerDueAmount.
        /// Stored here explicitly so the record is self-contained for auditing.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>Currency of the disbursement (e.g., "EGP").</summary>
        [Required, MaxLength(10)]
        public string Currency { get; set; } = "EGP";

        // ── State ─────────────────────────────────────────────────────────────────

        public PayoutStatus Status { get; set; } = PayoutStatus.Pending;

        // ── Paymob Disbursement Tracking ──────────────────────────────────────────

        /// <summary>
        /// The unique ID returned by Paymob's Disbursement API when the disbursement
        /// is created. Used to match incoming disbursement webhook payloads to this
        /// record. Null until the API call is made.
        /// </summary>
        [MaxLength(100)]
        public string? PaymobDisbursementId { get; set; }

        /// <summary>
        /// The human-readable reference or tracking number Paymob provides for the
        /// bank transfer. Populated after the disbursement webhook confirms the transfer.
        /// </summary>
        [MaxLength(200)]
        public string? PaymobDisbursementRef { get; set; }

        // ── Failure Tracking ──────────────────────────────────────────────────────

        /// <summary>
        /// Populated when Status transitions to Failed. Contains Paymob's rejection
        /// reason (e.g., "Invalid IBAN", "Account closed") or an internal error message.
        /// </summary>
        [MaxLength(500)]
        public string? FailureReason { get; set; }

        // ── Timestamps ────────────────────────────────────────────────────────────

        /// <summary>When the Paymob Disbursement API was called.</summary>
        public DateTime InitiatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When Paymob confirmed the transfer completed (or failed).
        /// Null while Status is Pending or Processing.
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        // ── Navigation ────────────────────────────────────────────────────────────

        public EscrowRecord EscrowRecord { get; set; } = null!;

        public Companies SellerCompany { get; set; } = null!;

        public SellerBankAccount DestinationAccount { get; set; } = null!;
    }
}
