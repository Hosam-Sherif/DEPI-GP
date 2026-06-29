using System.ComponentModel.DataAnnotations;

namespace Mazaad.Application.DTOs
{
    public class RejectCompanyDto
    {
        [Required, MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }
}