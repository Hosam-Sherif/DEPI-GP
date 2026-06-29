using System;
using System.ComponentModel.DataAnnotations;

namespace Mazaad.Application.DTOs
{
    public class IndustryResponseDto
    {
        public int Id { get; set; }
        public string IndustryName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateIndustryDto
    {
        [Required, MaxLength(150)]
        public string IndustryName { get; set; } = string.Empty;
    }

    public class UpdateIndustryDto
    {
        [Required, MaxLength(150)]
        public string IndustryName { get; set; } = string.Empty;
    }
}