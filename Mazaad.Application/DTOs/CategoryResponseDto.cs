using System;

namespace Mazaad.Application.DTOs
{
    public class CategoryResponseDto
    {
        public int Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string UnitOfMeasure { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}