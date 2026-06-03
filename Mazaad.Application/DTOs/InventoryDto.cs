public class InventoryDto
{
    public string ProductName { get; set; }

    public string Description { get; set; }

    public decimal StartingPrice { get; set; }

    public int Quantity { get; set; }

    public IFormFile Image { get; set; }
}