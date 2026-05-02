using System.Collections.Generic;
using System.Threading.Tasks;
using Mazaad.Application.DTOs;

namespace Mazaad.Application.Interfaces
{
    public interface ICompanyService
    {
        Task<IEnumerable<CompanyResponseDto>> GetAllCompaniesAsync();
        Task<CompanyResponseDto?> GetCompanyByIdAsync(int id);
        Task<CompanyResponseDto> CreateCompanyAsync(CreateCompanyDto request);
        Task<bool> VerifyCompanyAsync(int id);
    }
}
