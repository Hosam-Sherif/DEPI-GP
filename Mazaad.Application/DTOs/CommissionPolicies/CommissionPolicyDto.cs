namespace Mazaad.Application.DTOs.CommissionPolicies
{
    /// <summary>Response DTO returned to the client.</summary>
    public class CommissionPolicyDto
    {
        public int Id { get; set; }
        public string PolicyName { get; set; } = string.Empty;
        public decimal CommissionRate { get; set; }   // e.g. 0.05 = 5%
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime EffectiveTo { get; set; }
        public bool Active { get; set; }
        public int OrdersCount { get; set; }   // كم أوردر بيستخدم البوليسي ده
    }
}