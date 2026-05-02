using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public DateTime OrderDate { get; set; }

        public Companies SellerCompany { get; set; }
        public Companies BuyerCompany { get; set; }
        public Bids Bid { get; set; }
        public Commission_Policies AppliedPolicy { get; set; }
        public ICollection<Payments> Payments { get; set; } = new HashSet<Payments>();
    }
}
