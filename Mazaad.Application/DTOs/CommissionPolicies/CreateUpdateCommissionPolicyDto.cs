using System.ComponentModel.DataAnnotations;

namespace Mazaad.Application.DTOs.CommissionPolicies
{
    /// <summary>Payload for creating a new commission policy.</summary>
    public class CreateCommissionPolicyDto
    {
        [Required]
        [MaxLength(200)]
        public string PolicyName { get; set; } = string.Empty;

        /// <summary>Rate as a decimal fraction, e.g. 0.05 for 5%.</summary>
        [Required]
        [Range(0.0001, 1.0, ErrorMessage = "CommissionRate must be between 0.0001 and 1.0")]
        public decimal CommissionRate { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal MinAmount { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal MaxAmount { get; set; }

        [Required]
        public DateTime EffectiveFrom { get; set; }

        [Required]
        public DateTime EffectiveTo { get; set; }
    }

    /// <summary>Payload for updating an existing commission policy.</summary>
    public class UpdateCommissionPolicyDto
    {
        [Required]
        [MaxLength(200)]
        public string PolicyName { get; set; } = string.Empty;

        [Required]
        [Range(0.0001, 1.0, ErrorMessage = "CommissionRate must be between 0.0001 and 1.0")]
        public decimal CommissionRate { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal MinAmount { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal MaxAmount { get; set; }

        [Required]
        public DateTime EffectiveFrom { get; set; }

        [Required]
        public DateTime EffectiveTo { get; set; }
    }
}