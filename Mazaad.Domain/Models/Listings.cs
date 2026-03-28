using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mazaad.Domain.Models
{
    public class Listings
    {
        public int ID { get; set; }

        [ForeignKey("Company")]
        public int company_id { get; set; }

        [ForeignKey("Category")]
        public int category_id { get; set; }

        public string title { get; set; }
        public string description { get; set; }
        public decimal min_order_quantity { get; set; }
        public decimal available_quantity { get; set; }
        public decimal purity_percentage { get; set; }
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }

        public Companies Company { get; set; }
        public Material_Categories Category { get; set; }
        public ICollection<Bids> Bids { get; set; } = new HashSet<Bids>();
        public ICollection<Chat_Channels> Chat_Channels { get; set; } = new HashSet<Chat_Channels>();
    }
}
