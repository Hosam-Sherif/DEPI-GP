using Mazaad.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mazaad.Domain.Models
{
    public class InventoryItem
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Company))]
        public int company_id { get; set; }

        [ForeignKey(nameof(Category))]
        public int category_id { get; set; }

        public string name { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public decimal quantity { get; set; }
        public string unit_of_measure { get; set; } = string.Empty;
        public decimal minimum_auction_price { get; set; }
        public decimal? current_market_price { get; set; }
        public string? image_path { get; set; }
        public string? image_name { get; set; }
        public InventoryItemStatus status { get; set; } = InventoryItemStatus.Available;
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }

        public Companies Company { get; set; } = null!;
        public Material_Categories Category { get; set; } = null!;
    }
}
