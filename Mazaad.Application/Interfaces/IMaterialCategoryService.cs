using System.Collections.Generic;
using System.Threading.Tasks;
using Mazaad.Application.DTOs;

namespace Mazaad.Application.Interfaces
{
    public interface IMaterialCategoryService
    {
        Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync();
        Task<CategoryResponseDto> GetCategoryByIdAsync(int id);
        Task<CategoryResponseDto> CreateCategoryAsync(CreateCategoryDto request);
        Task<bool> DeleteCategoryAsync(int id);
    }
}