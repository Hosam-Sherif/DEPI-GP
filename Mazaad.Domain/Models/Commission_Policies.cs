using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Mazaad.Domain.Models
{
    public class Commission_Policies
    {
        [Key]
        public int Id { get; set; }

        public string PolicyName { get; set; }
        public decimal CommissionRate { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime EffectiveTo { get; set; }
        public bool Active { get; set; }

        public ICollection<Orders> AppliedOrders { get; set; } = new HashSet<Orders>();
    }
}