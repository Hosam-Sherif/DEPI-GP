using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mazaad.Domain.Models
{
    public class Companies
    {
        public int Id { get; set; }
        [ForeignKey("Industry")]
        public int industry_id { get; set; }
        public string company_name { get; set; }
        public string commercial_reg_num { get; set; }
        public string tax_registration_num { get; set; }
        public string city { get; set; }
        public string address_details { get; set; }
        public bool is_verified { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }

        public IndustryType Industry { get; set; }
        public ICollection<App_Users> Users { get; set; } = new HashSet<App_Users>();
        public ICollection<Listings> Listings { get; set; } = new HashSet<Listings>();
        public ICollection<Bids> Bids { get; set; } = new HashSet<Bids>();
        public virtual ICollection<Orders> SalesOrders { get; set; } = new HashSet<Orders>();
        public virtual ICollection<Orders> PurchaseOrders { get; set; } = new HashSet<Orders>();
        public virtual ICollection<Chat_Channels> SellerChatChannels { get; set; } = new HashSet<Chat_Channels>();
        public virtual ICollection<Chat_Channels> BuyerChatChannels { get; set; } = new HashSet<Chat_Channels>();
    }
}
