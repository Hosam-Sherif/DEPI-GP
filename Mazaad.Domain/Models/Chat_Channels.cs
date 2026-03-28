using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mazaad.Domain.Models
{
    public class Chat_Channels
    {
        public int Id { get; set; }

        [ForeignKey("Listing")]
        public int listing_id { get; set; }

        [ForeignKey("SellerCompany")]
        public int seller_company_id { get; set; }

        [ForeignKey("BuyerCompany")]
        public int buyer_company_id { get; set; }

        public string status { get; set; }
        public DateTime created_at { get; set; }

        public Listings Listing { get; set; }
        public Companies SellerCompany { get; set; }
        public Companies BuyerCompany { get; set; }
        public ICollection<Messages> Messages { get; set; } = new HashSet<Messages>();
    }
}
