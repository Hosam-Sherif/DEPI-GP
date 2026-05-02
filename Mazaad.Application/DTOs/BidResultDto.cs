namespace Mazaad.Application.DTOs
{
    public class BidResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string DisplayBiddersName { get; set; }
        public decimal NewPrice { get; set; }
    }
}