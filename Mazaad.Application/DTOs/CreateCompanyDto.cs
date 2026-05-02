using System.ComponentModel.DataAnnotations;

namespace Mazaad.Application.DTOs
{
    public class CreateCompanyDto
    {
        [Required]
        public int IndustryId { get; set; }

        [Required, MaxLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string CommercialRegNum { get; set; } = string.Empty;

        [MaxLength(100)]
        public string TaxRegistrationNum { get; set; } = string.Empty;

        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        public string AddressDetails { get; set; } = string.Empty;
    }
}
