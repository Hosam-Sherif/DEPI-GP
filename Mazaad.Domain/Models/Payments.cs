using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mazaad.Domain.Models
{
    public class Payments
    {
        public int Id { get; set; }
        [ForeignKey("Order")]
        public int order_id { get; set; }

        public decimal amount { get; set; }
        public string payment_method { get; set; }
        public string transaction_reference { get; set; }
        public string status { get; set; }
        public DateTime paid_at { get; set; }
        public DateTime created_at { get; set; }
        public Orders Order { get; set; }
    }

}
