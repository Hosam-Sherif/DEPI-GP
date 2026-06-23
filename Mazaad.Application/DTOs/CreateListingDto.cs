using System;
using System.ComponentModel.DataAnnotations;

namespace Mazaad.Application.DTOs
{
    public class CreateListingDto
    {
        public int CategoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal MinOrderQuantity { get; set; }
        public decimal AvailableQuantity { get; set; }

        /// <summary>
        /// OPTIONAL — ignored on save. The UnitOfMeasure is automatically
        /// inherited from the MaterialCategory (e.g. Steel → "Ton").
        /// Included for reference only.
        /// </summary>
        public string? UnitOfMeasure { get; set; }

        public decimal PurityPercentage { get; set; }
        public string BaseCurrency { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal StartingPrice { get; set; }
    }
}
