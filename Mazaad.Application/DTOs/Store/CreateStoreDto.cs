namespace Mazaad.Application.DTOs.Store
{
    public class CreateStoreDto
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Color { get; set; } = "#D4AF37";
        // اللوجو هيتبعت كـ IFormFile من الـ Controller
    }
}