// Mazaad.Application/Interfaces/Services/ICompanyUserService.cs

using Mazaad.Application.Common;
using Mazaad.Application.DTOs.Company;

namespace Mazaad.Application.Interfaces.Services
{
    public interface ICompanyUserService
    {
        // ── CompanyAdmin: يجيب كل users شركته ───────────
        Task<IEnumerable<CompanyUserResponseDto>> GetUsersAsync(int companyId);

        // ── CompanyAdmin: يضيف user جديد لشركته ─────────
        Task<Result<CompanyUserResponseDto>> AddUserAsync(int companyId, AddCompanyUserDto dto, string ipAddress);

        // ── CompanyAdmin: يعدل role أو يفعّل/يعطّل user ─
        Task<Result> UpdateUserAsync(int companyId, int userId, UpdateCompanyUserDto dto, string ipAddress);

        // ── CompanyAdmin: يحذف user من شركته ────────────
        Task<Result> RemoveUserAsync(int companyId, int userId, string ipAddress);
    }
}