using System.Collections.Generic;
using System.Threading.Tasks;
using Mazaad.Domain.Models;

namespace Mazaad.Application.Interfaces.Repositories
{
    public interface IInventoryRepository
    {
        Task AddAsync(Listings listing);
        Task<List<Listings>> GetCompanyInventoryAsync(int companyId);
        Task<Listings?> GetByIdAsync(int id);
        Task SaveChangesAsync();
        void Delete(Listings listing);
    }
}
