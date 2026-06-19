namespace Mazaad.Application.DTOs.Sales
{
    public class TopProductDto
    {
        public string ProductName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int QuantitySold { get; set; }
    }
}
