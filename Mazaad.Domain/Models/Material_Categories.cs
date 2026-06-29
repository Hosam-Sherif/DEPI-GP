using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Mazaad.Domain.Models
{
    public class Material_Categories
    {
        [Key]
        public int Id { get; set; }

        public string CategoryName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string UnitOfMeasure { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string image_url { get; set; } = string.Empty;

        [Timestamp]
        public byte[]? RowVersion { get; set; }

        public ICollection<Listings> Listings { get; set; } = new HashSet<Listings>();
    }
}