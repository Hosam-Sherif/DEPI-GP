using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Mazaad.Domain.Models
{
    public class Payments
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string TransactionReference { get; set; }
        public string Status { get; set; }
        public DateTime PaidAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public Orders Order { get; set; }
    }
}
