using Microsoft.AspNetCore.Http;

namespace Mazaad.Application.DTOs
{
    public class InventoryDto
    {
        public string ProductName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal StartingPrice { get; set; }

        public int Quantity { get; set; }

        public IFormFile? Image { get; set; }
    }
}
