using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Mazaad.Domain.Enums;

namespace Mazaad.Domain.Models
{
    public class Orders
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("SellerCompany")]
        public int SellerCompanyId { get; set; }

        [ForeignKey("BuyerCompany")]
        public int BuyerCompanyId { get; set; }

        [ForeignKey("Bid")]
        public int BidId { get; set; }

        [ForeignKey("AppliedPolicy")]
        public int AppliedPolicyId { get; set; }

        public decimal AgreedQuantity { get; set; }
        public decimal AgreedUnitPrice { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        public Companies SellerCompany { get; set; } = null!;
        public Companies BuyerCompany { get; set; } = null!;
        public Bids Bid { get; set; } = null!;
        public Commission_Policies AppliedPolicy { get; set; } = null!;
        public ICollection<Payments> Payments { get; set; } = new HashSet<Payments>();
    }
}
