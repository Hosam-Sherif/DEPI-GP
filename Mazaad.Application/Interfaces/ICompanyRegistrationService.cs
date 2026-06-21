// Mazaad.Application/Interfaces/Services/ICompanyRegistrationService.cs

using Mazaad.Application.Common;
using Mazaad.Application.DTOs.Auth;
using Mazaad.Application.DTOs.Company;

namespace Mazaad.Application.Interfaces.Services
{
    public interface ICompanyRegistrationService
    {
        // ── تسجيل شركة جديدة + أول Admin ليها ──────────
        Task<Result<AuthResponseDto>> RegisterCompanyAsync(RegisterCompanyDto dto, string ipAddress);

        // ── SuperAdmin: يجيب قائمة الشركات المنتظرة ─────
        Task<IEnumerable<PendingCompanyDto>> GetPendingCompaniesAsync();

        // ── SuperAdmin: يوافق أو يرفض شركة ─────────────
        Task<Result> VerifyCompanyAsync(int companyId, int adminUserId, VerifyCompanyDto dto, string ipAddress);
    }
}