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
    public class CompanyService : ICompanyService
    {
        private readonly AppDbContext _context;

        public CompanyService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CompanyResponseDto>> GetAllCompaniesAsync()
        {
            var companies = await _context.Companies
                .Include(c => c.Industry)
                .OrderBy(c => c.CompanyName)
                .ToListAsync();

            return companies.Select(MapToDto);
        }

        public async Task<CompanyResponseDto?> GetCompanyByIdAsync(int id)
        {
            var company = await _context.Companies
                .Include(c => c.Industry)
                .FirstOrDefaultAsync(c => c.Id == id);

            return company == null ? null : MapToDto(company);
        }

        public async Task<CompanyResponseDto> CreateCompanyAsync(CreateCompanyDto request)
        {
            var company = new Companies
            {
                IndustryId = request.IndustryId,
                CompanyName = request.CompanyName,
                CommercialRegNum = request.CommercialRegNum,
                TaxRegistrationNum = request.TaxRegistrationNum,
                City = request.City,
                AddressDetails = request.AddressDetails,
                IsVerified = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            // Load navigation for response
            await _context.Entry(company).Reference(c => c.Industry).LoadAsync();

            return MapToDto(company);
        }

        public async Task<bool> VerifyCompanyAsync(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null) return false;

            company.IsVerified = true;
            company.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        private static CompanyResponseDto MapToDto(Companies c) => new CompanyResponseDto
        {
            Id = c.Id,
            IndustryId = c.IndustryId,
            IndustryName = c.Industry?.IndustryName ?? string.Empty,
            CompanyName = c.CompanyName,
            CommercialRegNum = c.CommercialRegNum,
            TaxRegistrationNum = c.TaxRegistrationNum,
            City = c.City,
            AddressDetails = c.AddressDetails,
            IsVerified = c.IsVerified,
            CreatedAt = c.CreatedAt
        };
    }
}
