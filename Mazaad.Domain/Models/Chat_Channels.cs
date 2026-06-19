using Mazaad.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mazaad.Domain.Models
{
    public class Chat_Channels
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Listing")]
        public int ListingId { get; set; }

        [ForeignKey("SellerCompany")]
        public int SellerCompanyId { get; set; }

        [ForeignKey("BuyerCompany")]
        public int BuyerCompanyId { get; set; }
        public ChannelStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public Listings Listing { get; set; } = null!;
        public Companies SellerCompany { get; set; } = null!;
        public Companies BuyerCompany { get; set; } = null!;
        public ICollection<Messages> Messages { get; set; } = new HashSet<Messages>();
    }
}
