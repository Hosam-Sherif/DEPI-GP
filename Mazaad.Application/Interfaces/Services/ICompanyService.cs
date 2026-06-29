using System.Collections.Generic;
using System.Threading.Tasks;
using Mazaad.Application.DTOs;

namespace Mazaad.Application.Interfaces.Services
{
    public interface ICompanyService
    {
        Task<IEnumerable<CompanyResponseDto>> GetAllCompaniesAsync();
        Task<IEnumerable<CompanyResponseDto>> GetPendingCompaniesAsync();

        /// <summary>الشركات الموثقة فقط — بدون login (للصفحة العامة /companies).</summary>
        Task<IEnumerable<CompanyPublicDto>> GetVerifiedCompaniesAsync();

        Task<CompanyResponseDto?> GetCompanyByIdAsync(int id);
        Task<CompanyResponseDto> CreateCompanyAsync(CreateCompanyDto request);
        Task<bool> VerifyCompanyAsync(int id, int verifiedByUserId);
        Task<bool> RejectCompanyAsync(int id, string reason, int verifiedByUserId);
    }
}