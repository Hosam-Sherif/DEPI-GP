using System;

namespace Mazaad.Application.DTOs
{
    public class CreateListingDto
    {
        public int CategoryId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal MinOrderQuantity { get; set; }
        public decimal AvailableQuantity { get; set; }
        public decimal PurityPercentage { get; set; }
        public string BaseCurrency { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal StartingPrice { get; set; }
    }
}