// Mazaad.Application/Interfaces/Services/ICompanyRegistrationService.cs

using Mazaad.Application.Common;
using Mazaad.Application.DTOs.Company;

namespace Mazaad.Application.Interfaces.Services
{
    public interface ICompanyRegistrationService
    {
        // بيرجع بيانات pending بس — من غير أي access token
        Task<Result<CompanyRegistrationResultDto>> RegisterCompanyAsync(
            RegisterCompanyDto dto, string ipAddress);

        Task<IEnumerable<PendingCompanyDto>> GetPendingCompaniesAsync();

        Task<Result> VerifyCompanyAsync(
            int companyId, int adminUserId, VerifyCompanyDto dto, string ipAddress);
    }
}