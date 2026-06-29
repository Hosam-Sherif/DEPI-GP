// Mazaad.Infrastructure/Services/Auth/ProfileService.cs

using Mazaad.Application.Common;
using Mazaad.Application.DTOs.Auth;
using Mazaad.Application.Interfaces.Services;
using Mazaad.Domain.Enums;
using Mazaad.Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace Mazaad.Infrastructure.Services.Auth
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISecurityLogService _securityLog;

        public ProfileService(
            UserManager<ApplicationUser> userManager,
            ISecurityLogService securityLog)
        {
            _userManager = userManager;
            _securityLog = securityLog;
        }

        public async Task<Result<MyProfileDto>> GetMyProfileAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return Result<MyProfileDto>.Failure("User not found.");

            return Result<MyProfileDto>.Success(MapToDto(user));
        }

        public async Task<Result<MyProfileDto>> UpdateMyProfileAsync(
            int userId,
            UpdateProfileDto dto,
            string ipAddress)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return Result<MyProfileDto>.Failure("User not found.");

            user.FullName = dto.FullName;
            user.JobTitle = dto.JobTitle;
            user.PhoneNumber = dto.PhoneNumber;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = updateResult.Errors.Select(e => e.Description);
                return Result<MyProfileDto>.Failure(errors.ToArray());
            }

            // نسجل التعديل في الـ Security Log (مفيد لو حد غير بيانات حساسة)
            await _securityLog.LogAsync(
                SecurityEventType.ProfileUpdated,
                success: true,
                ipAddress: ipAddress,
                userId: userId,
                email: user.Email);

            return Result<MyProfileDto>.Success(MapToDto(user));
        }

        private static MyProfileDto MapToDto(ApplicationUser user) => new()
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            JobTitle = user.JobTitle,
            PhoneNumber = user.PhoneNumber,
            CompanyId = user.CompanyId,
            TwoFactorEnabled = user.TwoFactorEnabled,
            LastLoginDate = user.LastLoginDate
        };
    }
}