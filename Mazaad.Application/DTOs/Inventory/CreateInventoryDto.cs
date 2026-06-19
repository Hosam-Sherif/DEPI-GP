using Microsoft.AspNetCore.Http;

namespace Mazaad.Application.DTOs.Inventory
{
    public class CreateInventoryDto
    {
        public string ProductName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal StartingPrice { get; set; }
        public decimal Quantity { get; set; }
        public IFormFile Image { get; set; } = null!;
    }
}
