using Mazaad.Application.DTOs.CommissionPolicies;
using Mazaad.Application.Interfaces.Services;
using Mazaad.Domain.Models;
using Mazaad.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mazaad.Infrastructure.Services
{
    public class CommissionPolicyService : ICommissionPolicyService
    {
        private readonly AppDbContext _db;

        public CommissionPolicyService(AppDbContext db)
        {
            _db = db;
        }

        // ── GET ALL ───────────────────────────────────────────────────────────
        public async Task<IEnumerable<CommissionPolicyDto>> GetAllAsync()
        {
            return await _db.CommissionPolicies
                .AsNoTracking()
                .OrderByDescending(p => p.EffectiveFrom)
                .Select(p => new CommissionPolicyDto
                {
                    Id = p.Id,
                    PolicyName = p.PolicyName,
                    CommissionRate = p.CommissionRate,
                    MinAmount = p.MinAmount,
                    MaxAmount = p.MaxAmount,
                    EffectiveFrom = p.EffectiveFrom,
                    EffectiveTo = p.EffectiveTo,
                    Active = p.Active,
                    OrdersCount = p.AppliedOrders.Count()
                })
                .ToListAsync();
        }

        // ── GET BY ID ─────────────────────────────────────────────────────────
        public async Task<CommissionPolicyDto?> GetByIdAsync(int id)
        {
            return await _db.CommissionPolicies
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new CommissionPolicyDto
                {
                    Id = p.Id,
                    PolicyName = p.PolicyName,
                    CommissionRate = p.CommissionRate,
                    MinAmount = p.MinAmount,
                    MaxAmount = p.MaxAmount,
                    EffectiveFrom = p.EffectiveFrom,
                    EffectiveTo = p.EffectiveTo,
                    Active = p.Active,
                    OrdersCount = p.AppliedOrders.Count()
                })
                .FirstOrDefaultAsync();
        }

        // ── CREATE ────────────────────────────────────────────────────────────
        public async Task<CommissionPolicyDto> CreateAsync(CreateCommissionPolicyDto dto)
        {
            ValidateDateRange(dto.EffectiveFrom, dto.EffectiveTo);
            ValidateAmountRange(dto.MinAmount, dto.MaxAmount);

            var policy = new Commission_Policies
            {
                PolicyName = dto.PolicyName.Trim(),
                CommissionRate = dto.CommissionRate,
                MinAmount = dto.MinAmount,
                MaxAmount = dto.MaxAmount,
                EffectiveFrom = dto.EffectiveFrom.ToUniversalTime(),
                EffectiveTo = dto.EffectiveTo.ToUniversalTime(),
                Active = true   // كل policy جديدة تبدأ active
            };

            _db.CommissionPolicies.Add(policy);
            await _db.SaveChangesAsync();

            return new CommissionPolicyDto
            {
                Id = policy.Id,
                PolicyName = policy.PolicyName,
                CommissionRate = policy.CommissionRate,
                MinAmount = policy.MinAmount,
                MaxAmount = policy.MaxAmount,
                EffectiveFrom = policy.EffectiveFrom,
                EffectiveTo = policy.EffectiveTo,
                Active = policy.Active,
                OrdersCount = 0
            };
        }

        // ── UPDATE ────────────────────────────────────────────────────────────
        public async Task<bool> UpdateAsync(int id, UpdateCommissionPolicyDto dto)
        {
            var policy = await _db.CommissionPolicies.FindAsync(id);
            if (policy is null) return false;

            ValidateDateRange(dto.EffectiveFrom, dto.EffectiveTo);
            ValidateAmountRange(dto.MinAmount, dto.MaxAmount);

            policy.PolicyName = dto.PolicyName.Trim();
            policy.CommissionRate = dto.CommissionRate;
            policy.MinAmount = dto.MinAmount;
            policy.MaxAmount = dto.MaxAmount;
            policy.EffectiveFrom = dto.EffectiveFrom.ToUniversalTime();
            policy.EffectiveTo = dto.EffectiveTo.ToUniversalTime();

            await _db.SaveChangesAsync();
            return true;
        }

        // ── DEACTIVATE ────────────────────────────────────────────────────────
        public async Task<bool> DeactivateAsync(int id)
        {
            var policy = await _db.CommissionPolicies.FindAsync(id);
            if (policy is null || !policy.Active) return false;

            policy.Active = false;
            await _db.SaveChangesAsync();
            return true;
        }

        // ── Private Validators ────────────────────────────────────────────────
        private static void ValidateDateRange(DateTime from, DateTime to)
        {
            if (to <= from)
                throw new ArgumentException("EffectiveTo must be after EffectiveFrom.");
        }

        private static void ValidateAmountRange(decimal min, decimal max)
        {
            if (max < min)
                throw new ArgumentException("MaxAmount must be greater than or equal to MinAmount.");
        }
    }
}