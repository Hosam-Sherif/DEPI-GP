using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Mazaad.Domain.Models
{
    public class Material_Categories
    {
        [Key]
        public int Id { get; set; }

        public string CategoryName { get; set; }
        public string Description { get; set; }
        public string UnitOfMeasure { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<Listings> Listings { get; set; } = new HashSet<Listings>();
    }
}
