using System.ComponentModel.DataAnnotations;

namespace Mazaad.Application.DTOs
{
    /// <summary>Request DTO for finalizing a winning bid into an Order.</summary>
    public class FinalizeOrderDto
    {
        [Required]
        public int BidId { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;
    }
}
