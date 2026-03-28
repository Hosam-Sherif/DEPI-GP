using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mazaad.Domain.Models
{
    public class Bids
    {
        public int Id { get; set; }

        [ForeignKey("Listing")]
        public int listing_id { get; set; }

        [ForeignKey("User")]
        public int placed_by_user_id { get; set; }

        [ForeignKey("BuyerCompany")]
        public int buyer_company_id { get; set; }

        public decimal bid_amount_per_unit { get; set; }
        public decimal total_bid_amount { get; set; }
        public bool winning_bid { get; set; }

        public Listings Listing { get; set; }
        public App_Users User { get; set; }
        public Companies BuyerCompany { get; set; }

        public ICollection<Orders> Orders { get; set; } = new HashSet<Orders>();
    }
}
