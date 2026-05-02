using System;

namespace Mazaad.Application.DTOs
{
    public class IndustryResponseDto
    {
        public int Id { get; set; }
        public string IndustryName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
