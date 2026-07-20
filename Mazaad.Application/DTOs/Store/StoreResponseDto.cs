namespace Mazaad.Application.DTOs.Store
{
    public class StoreResponseDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string StoreUrl { get; set; } = string.Empty; // Full URL
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string Color { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}