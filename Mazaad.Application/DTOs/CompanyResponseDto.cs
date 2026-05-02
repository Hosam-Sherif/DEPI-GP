using System;

namespace Mazaad.Application.DTOs
{
    /// <summary>Company summary response DTO.</summary>
    public class CompanyResponseDto
    {
        public int Id { get; set; }
        public int IndustryId { get; set; }
        public string IndustryName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string CommercialRegNum { get; set; } = string.Empty;
        public string TaxRegistrationNum { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string AddressDetails { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
