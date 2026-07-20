using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Mazaad.Domain.Enums;

namespace Mazaad.Domain.Models
{
    public class Companies
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Industry")]
        public int IndustryId { get; set; }

        public string CompanyName { get; set; } = string.Empty;
        public string CommercialRegNum { get; set; } = string.Empty;
        public string TaxRegistrationNum { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string AddressDetails { get; set; } = string.Empty;

        // ── Verification ───────────────────────────────────────────────────────

        /// <summary>
        /// Replaces the old bool IsVerified.
        /// Supports the full state machine: Pending → Verified | Rejected | Suspended.
        /// </summary>
        public CompanyVerificationStatus VerificationStatus { get; set; }
            = CompanyVerificationStatus.Pending;

        /// <summary>Set by admin on verification or rejection.</summary>
        public int? VerifiedByUserId { get; set; }

        public DateTime? VerifiedAt { get; set; }

        /// <summary>Populated when status = Rejected or Suspended.</summary>
        public string? RejectionReason { get; set; }

        // ── Timestamps ─────────────────────────────────────────────────────────

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ── Navigation ─────────────────────────────────────────────────────────

        public IndustryType Industry { get; set; } = null!;

        /// <summary>All users that belong to this company.</summary>
        public ICollection<ApplicationUser> Users { get; set; } = new HashSet<ApplicationUser>();

        public ICollection<Listings> Listings { get; set; } = new HashSet<Listings>();
        public ICollection<Bids> Bids { get; set; } = new HashSet<Bids>();
        public Store? Store { get; set; }

        public virtual ICollection<Orders> SalesOrders { get; set; } = new HashSet<Orders>();
        public virtual ICollection<Orders> PurchaseOrders { get; set; } = new HashSet<Orders>();

        public virtual ICollection<Chat_Channels> SellerChatChannels { get; set; } = new HashSet<Chat_Channels>();
        public virtual ICollection<Chat_Channels> BuyerChatChannels { get; set; } = new HashSet<Chat_Channels>();

        // ── Computed helpers ───────────────────────────────────────────────────

        public bool IsVerified => VerificationStatus == CompanyVerificationStatus.Verified;
        public bool IsActive => VerificationStatus != CompanyVerificationStatus.Rejected
                             && VerificationStatus != CompanyVerificationStatus.Suspended;
    }
}