using Mazaad.Application.Common;
using Mazaad.Application.DTOs.Company;

namespace Mazaad.Application.Interfaces.Services
{
    public interface ICompanyUserService
    {
        // الـ Methods القديمة زي ما هي
        Task<IEnumerable<CompanyUserResponseDto>> GetUsersAsync(int companyId);
        Task<Result<CompanyUserResponseDto>> AddUserAsync(int companyId, AddCompanyUserDto dto, string ipAddress);
        Task<Result> UpdateUserAsync(int companyId, int userId, UpdateCompanyUserDto dto, string ipAddress);
        Task<Result> RemoveUserAsync(int companyId, int userId, string ipAddress);

        // ── الـ Methods الجديدة ───────────────────────────────────────────

        // يجيب بيانات يوزر واحد محدد
        Task<CompanyUserResponseDto?> GetUserByIdAsync(int companyId, int userId);

    }
}