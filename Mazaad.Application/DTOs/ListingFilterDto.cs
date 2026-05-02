using Mazaad.Domain.Enums;

namespace Mazaad.Application.DTOs
{
    /// <summary>Query parameters for the marketplace listing endpoint with filtering and pagination.</summary>
    public class ListingFilterDto
    {
        public int? CategoryId { get; set; }
        public ListingCondition? Condition { get; set; }
        public ListingStatus? Status { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        /// <summary>Free-text search on Title / Description</summary>
        public string? SearchTerm { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 9;
    }
}
