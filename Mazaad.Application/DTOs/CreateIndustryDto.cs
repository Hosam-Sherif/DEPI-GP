using System.ComponentModel.DataAnnotations;

namespace Mazaad.Application.DTOs
{
    public class CreateIndustryDto
    {
        [Required, MaxLength(150)]
        public string IndustryName { get; set; } = string.Empty;
    }
}
