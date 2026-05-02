using System.Collections.Generic;
using System.Threading.Tasks;
using Mazaad.Application.DTOs;

namespace Mazaad.Application.Interfaces
{
    public interface IIndustryService
    {
        Task<IEnumerable<IndustryResponseDto>> GetAllIndustriesAsync();
        Task<IndustryResponseDto?> GetIndustryByIdAsync(int id);
        Task<IndustryResponseDto> CreateIndustryAsync(CreateIndustryDto request);
        Task<bool> DeleteIndustryAsync(int id);
    }
}
