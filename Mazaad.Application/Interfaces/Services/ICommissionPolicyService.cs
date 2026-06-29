using Mazaad.Application.DTOs.CommissionPolicies;

namespace Mazaad.Application.Interfaces.Services
{
    public interface ICommissionPolicyService
    {
        /// <summary>Returns all policies (active + inactive), ordered by EffectiveFrom desc.</summary>
        Task<IEnumerable<CommissionPolicyDto>> GetAllAsync();

        /// <summary>Returns a single policy by ID, or null if not found.</summary>
        Task<CommissionPolicyDto?> GetByIdAsync(int id);

        /// <summary>Creates a new commission policy. Returns the created DTO.</summary>
        Task<CommissionPolicyDto> CreateAsync(CreateCommissionPolicyDto dto);

        /// <summary>Updates an existing policy. Returns false if not found.</summary>
        Task<bool> UpdateAsync(int id, UpdateCommissionPolicyDto dto);

        /// <summary>
        /// Soft-deactivates a policy (Active = false).
        /// Returns false if not found or already inactive.
        /// </summary>
        Task<bool> DeactivateAsync(int id);
    }
}