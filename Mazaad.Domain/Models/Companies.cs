using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mazaad.Domain.Models
{
    public class Companies
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Industry")]
        public int IndustryId { get; set; }

        public string CompanyName { get; set; }
        public string CommercialRegNum { get; set; }
        public string TaxRegistrationNum { get; set; }
        public string City { get; set; }
        public string AddressDetails { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

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
