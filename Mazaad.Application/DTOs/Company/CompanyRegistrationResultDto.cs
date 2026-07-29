// Mazaad.Application/DTOs/Company/CompanyRegistrationResultDto.cs

namespace Mazaad.Application.DTOs.Company
{
    /// <summary>
    /// بيرجع بعد تسجيل شركة جديدة — بدون أي access/refresh token،
    /// لأن الشركة لسه Pending ومش مسموح لها تستخدم الـ API.
    /// </summary>
    public class CompanyRegistrationResultDto
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public int AdminUserId { get; set; }
        public string AdminEmail { get; set; } = string.Empty;
        public string Status { get; set; } = "PendingVerification";
    }
}