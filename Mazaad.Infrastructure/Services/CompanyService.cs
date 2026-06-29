using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mazaad.Application.DTOs;
using Mazaad.Application.Interfaces.Services;
using Mazaad.Domain.Enums;
using Mazaad.Domain.Models;
using Mazaad.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Mazaad.Infrastructure.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CompanyService(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ── Public endpoint (no auth) ─────────────────────────────────────────
        public async Task<IEnumerable<CompanyPublicDto>> GetVerifiedCompaniesAsync()
        {
            var companies = await _context.Companies
                .Include(c => c.Industry)
                .Where(c => c.VerificationStatus == CompanyVerificationStatus.Verified)
                .OrderBy(c => c.CompanyName)
                .ToListAsync();

            // نجيب عدد المزادات النشطة لكل شركة في query واحدة
            var companyIds = companies.Select(c => c.Id).ToList();

            var activeCountsMap = await _context.Listings
                .Where(l => companyIds.Contains(l.CompanyId) && l.Status == ListingStatus.Active)
                .GroupBy(l => l.CompanyId)
                .Select(g => new { CompanyId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CompanyId, x => x.Count);

            return companies.Select(c => new CompanyPublicDto
            {
                Id = c.Id,
                CompanyName = c.CompanyName,
                IndustryName = c.Industry?.IndustryName ?? string.Empty,
                City = c.City,
                ActiveListingsCount = activeCountsMap.TryGetValue(c.Id, out var count) ? count : 0,
                VerifiedAt = c.VerifiedAt ?? c.CreatedAt
            });
        }

        // ── Admin endpoints ───────────────────────────────────────────────────
        public async Task<IEnumerable<CompanyResponseDto>> GetAllCompaniesAsync()
        {
            var companies = await _context.Companies
                .Include(c => c.Industry)
                .Include(c => c.Users)
                .OrderBy(c => c.CompanyName)
                .ToListAsync();

            var result = new List<CompanyResponseDto>();
            foreach (var c in companies)
                result.Add(await MapToDtoAsync(c));

            return result;
        }

        public async Task<IEnumerable<CompanyResponseDto>> GetPendingCompaniesAsync()
        {
            var companies = await _context.Companies
                .Include(c => c.Industry)
                .Include(c => c.Users)
                .Where(c => c.VerificationStatus == CompanyVerificationStatus.Pending)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            var result = new List<CompanyResponseDto>();
            foreach (var c in companies)
                result.Add(await MapToDtoAsync(c));

            return result;
        }

        public async Task<CompanyResponseDto?> GetCompanyByIdAsync(int id)
        {
            var company = await _context.Companies
                .Include(c => c.Industry)
                .Include(c => c.Users)
                .FirstOrDefaultAsync(c => c.Id == id);

            return company == null ? null : await MapToDtoAsync(company);
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
                VerificationStatus = CompanyVerificationStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            await _context.Entry(company).Reference(c => c.Industry).LoadAsync();

            return await MapToDtoAsync(company);
        }

        public async Task<bool> VerifyCompanyAsync(int id, int verifiedByUserId)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null) return false;

            company.VerificationStatus = CompanyVerificationStatus.Verified;
            company.RejectionReason = null;
            company.VerifiedByUserId = verifiedByUserId;
            company.VerifiedAt = DateTime.UtcNow;
            company.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectCompanyAsync(int id, string reason, int verifiedByUserId)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null) return false;

            company.VerificationStatus = CompanyVerificationStatus.Rejected;
            company.RejectionReason = reason;
            company.VerifiedByUserId = verifiedByUserId;
            company.VerifiedAt = DateTime.UtcNow;
            company.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ── Mapper ────────────────────────────────────────────────────────────
        private async Task<CompanyResponseDto> MapToDtoAsync(Companies c)
        {
            string? adminName = null;
            string? adminEmail = null;

            foreach (var user in c.Users)
            {
                if (await _userManager.IsInRoleAsync(user, "CompanyAdmin"))
                {
                    adminName = user.FullName;
                    adminEmail = user.Email;
                    break;
                }
            }

            return new CompanyResponseDto
            {
                Id = c.Id,
                IndustryId = c.IndustryId,
                IndustryName = c.Industry?.IndustryName ?? string.Empty,
                CompanyName = c.CompanyName,
                CommercialRegNum = c.CommercialRegNum,
                TaxRegistrationNum = c.TaxRegistrationNum,
                City = c.City,
                AddressDetails = c.AddressDetails,
                VerificationStatus = c.VerificationStatus.ToString(),
                RejectionReason = c.RejectionReason,
                IsVerified = c.VerificationStatus == CompanyVerificationStatus.Verified,
                CreatedAt = c.CreatedAt,
                AdminFullName = adminName,
                AdminEmail = adminEmail
            };
        }
    }
}