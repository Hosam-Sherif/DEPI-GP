using Mazaad.Domain.Models;
using Mazaad.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mazaad.Infrastructure.Persistence.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly AppDbContext _context;

        public InventoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Listings listing)
        {
            await _context.Listings.AddAsync(listing);
        }

        public async Task<List<Listings>>
            GetCompanyInventoryAsync(int companyId)
        {
            return await _context.Listings
                .Where(x => x.company_id == companyId)
                .OrderByDescending(x => x.created_at)
                .ToListAsync();
        }

        public async Task<Listings?> GetByIdAsync(int id)
        {
            return await _context.Listings
                .FirstOrDefaultAsync(x => x.ID == id);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Delete(Listings listing)
        {
            _context.Listings.Remove(listing);
        }
    }
}