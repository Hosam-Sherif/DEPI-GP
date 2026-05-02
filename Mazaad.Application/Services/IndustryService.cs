using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mazaad.Application.DTOs;
using Mazaad.Application.Interfaces;
using Mazaad.Domain.Models;
using Mazaad.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mazaad.Application.Services
{
    public class IndustryService : IIndustryService
    {
        private readonly AppDbContext _context;

        public IndustryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<IndustryResponseDto>> GetAllIndustriesAsync()
        {
            var industries = await _context.IndustryTypes
                .Where(i => !i.IsDeleted)
                .OrderBy(i => i.IndustryName)
                .ToListAsync();

            return industries.Select(MapToDto);
        }

        public async Task<IndustryResponseDto?> GetIndustryByIdAsync(int id)
        {
            var industry = await _context.IndustryTypes
                .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);

            return industry == null ? null : MapToDto(industry);
        }

        public async Task<IndustryResponseDto> CreateIndustryAsync(CreateIndustryDto request)
        {
            var industry = new IndustryType
            {
                IndustryName = request.IndustryName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.IndustryTypes.Add(industry);
            await _context.SaveChangesAsync();

            return MapToDto(industry);
        }

        public async Task<bool> DeleteIndustryAsync(int id)
        {
            var industry = await _context.IndustryTypes.FindAsync(id);
            if (industry == null) return false;

            industry.IsDeleted = true;
            industry.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        private static IndustryResponseDto MapToDto(IndustryType i) => new IndustryResponseDto
        {
            Id = i.Id,
            IndustryName = i.IndustryName,
            CreatedAt = i.CreatedAt
        };
    }
}
