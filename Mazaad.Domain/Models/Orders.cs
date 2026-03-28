using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mazaad.Domain.Models
{
    public class Orders
    {
        public int Id { get; set; }

        [ForeignKey("SellerCompany")]
        public int seller_company_id { get; set; }

        [ForeignKey("BuyerCompany")]
        public int buyer_company_id { get; set; }

        [ForeignKey("Bid")]
        public int bid_id { get; set; }
        [ForeignKey("AppliedPolicy")]
        public int applied_policy_id { get; set; }
        public decimal agreed_quantity { get; set; }
        public decimal agreed_unit_price { get; set; }
        public decimal platform_fee { get; set; }
        public DateTime order_date { get; set; }

        public Companies SellerCompany { get; set; }
        public Companies BuyerCompany { get; set; }
        public Bids Bid { get; set; }
        public Commission_Policies AppliedPolicy { get; set; }
        public ICollection<Payments> Payments { get; set; } = new HashSet<Payments>();
    }
}
