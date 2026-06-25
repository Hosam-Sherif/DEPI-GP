using System.Threading.Tasks;
using Mazaad.Application.DTOs.Inventory;

namespace Mazaad.Application.Interfaces.Services
{
    public interface IInventoryService
    {
        Task<object> CreateAsync(int companyId, CreateInventoryDto dto);
        Task<object> UpdateAsync(int id, UpdateInventoryDto dto);
        Task DeleteAsync(int id);
        Task<object> GetCompanyInventoryAsync(int companyId);
    }
}
