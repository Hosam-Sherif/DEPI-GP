// Mazaad.Infrastructure/Services/Auth/CompanyUserService.cs

using Mazaad.Application.Common;
using Mazaad.Application.DTOs.Company;
using Mazaad.Application.Interfaces.Services;
using Mazaad.Domain.Enums;
using Mazaad.Domain.Models;
using Mazaad.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Mazaad.Infrastructure.Services.Auth
{
    public class CompanyUserService : ICompanyUserService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISecurityLogService _securityLog;

        private static readonly string[] AllowedRoles = { "CompanyAdmin", "CompanyUser" };

        public CompanyUserService(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            ISecurityLogService securityLog)
        {
            _context = context;
            _userManager = userManager;
            _securityLog = securityLog;
        }

        public async Task<CompanyUserResponseDto?> GetUserByIdAsync(int companyId, int userId)
        {
            // بنجيب اليوزر بناءً على الـ Id والـ CompanyId للتأكد إنه تبع الشركة دي
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.CompanyId == companyId);

            if (user == null)
                return null;

            // بنجيب الـ Roles الخاصة باليوزر ده عن طريق الـ UserManager
            var roles = await _userManager.GetRolesAsync(user);

            // بنعمل Map للـ Entity ونرجعها DTO باستخدام الـ Helper ميثود بتاعتك
            return MapToDto(user, roles);
        }
        // ── Get Company Users ─────────────────────────────────────────────────
        public async Task<IEnumerable<CompanyUserResponseDto>> GetUsersAsync(int companyId)
        {
            var users = await _context.Users
                .Where(u => u.CompanyId == companyId)
                .ToListAsync();

            var result = new List<CompanyUserResponseDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add(MapToDto(user, roles));
            }

            return result;
        }

        // ── Add User ──────────────────────────────────────────────────────────
        public async Task<Result<CompanyUserResponseDto>> AddUserAsync(
            int companyId,
            AddCompanyUserDto dto,
            string ipAddress)
        {
            // تحقق إن الـ role مسموح بيه
            if (!AllowedRoles.Contains(dto.Role))
                return Result<CompanyUserResponseDto>.Failure(
                    $"Invalid role. Allowed: {string.Join(", ", AllowedRoles)}");

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return Result<CompanyUserResponseDto>.Failure("Email already registered.");

            var user = new ApplicationUser
            {
                FullName = dto.FullName,
                Email = dto.Email,
                UserName = dto.Email,
                JobTitle = dto.JobTitle,
                CompanyId = companyId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
                return Result<CompanyUserResponseDto>.Failure(
                    createResult.Errors.Select(e => e.Description));

            await _userManager.AddToRoleAsync(user, dto.Role);

            await _securityLog.LogAsync(
                SecurityEventType.UserAddedToCompany,
                success: true,
                ipAddress: ipAddress,
                userId: user.Id,
                email: user.Email,
                details: $"Company: {companyId} | Role: {dto.Role}");

            var roles = await _userManager.GetRolesAsync(user);
            return Result<CompanyUserResponseDto>.Success(MapToDto(user, roles));
        }

        // ── Update User ───────────────────────────────────────────────────────
        public async Task<Result> UpdateUserAsync(
            int companyId,
            int userId,
            UpdateCompanyUserDto dto,
            string ipAddress)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.CompanyId == companyId);

            if (user == null)
                return Result.Failure("User not found in this company.");

            // تغيير الـ Role
            if (!string.IsNullOrEmpty(dto.Role))
            {
                if (!AllowedRoles.Contains(dto.Role))
                    return Result.Failure($"Invalid role.");

                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, dto.Role);

                await _securityLog.LogAsync(
                    SecurityEventType.UserRoleChangedInCompany,
                    success: true,
                    ipAddress: ipAddress,
                    userId: userId,
                    details: $"New role: {dto.Role}");
            }

            // تفعيل / تعطيل
            if (dto.IsActive.HasValue)
            {
                user.IsActive = dto.IsActive.Value;
                user.UpdatedAt = DateTime.UtcNow;

                await _userManager.UpdateAsync(user);

                await _securityLog.LogAsync(
                    dto.IsActive.Value
                        ? SecurityEventType.AccountActivated
                        : SecurityEventType.AccountDeactivated,
                    success: true,
                    ipAddress: ipAddress,
                    userId: userId);
            }

            return Result.Success();
        }

        // ── Remove User ───────────────────────────────────────────────────────
        public async Task<Result> RemoveUserAsync(
            int companyId,
            int userId,
            string ipAddress)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.CompanyId == companyId);

            if (user == null)
                return Result.Failure("User not found in this company.");

            // مش بنحذف — بنعطل بس (soft delete)
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);

            await _securityLog.LogAsync(
                SecurityEventType.UserRemovedFromCompany,
                success: true,
                ipAddress: ipAddress,
                userId: userId,
                email: user.Email,
                details: $"Company: {companyId}");

            return Result.Success();
        }

        // ── Helper ────────────────────────────────────────────────────────────
        private static CompanyUserResponseDto MapToDto(
            ApplicationUser user,
            IList<string> roles) => new()
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                JobTitle = user.JobTitle,
                Roles = roles,
                IsActive = user.IsActive,
                TwoFactorEnabled = user.TwoFactorEnabled,
                LastLoginDate = user.LastLoginDate,
                CreatedAt = user.CreatedAt
            };
    }
}