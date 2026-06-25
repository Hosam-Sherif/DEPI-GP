// Mazaad.Infrastructure/Services/Auth/AuthService.cs

using Mazaad.Application.Common;
using Mazaad.Application.DTOs.Auth;
using Mazaad.Application.Interfaces.Services;
using Mazaad.Domain.Enums;
using Mazaad.Domain.Models;
using Mazaad.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Mazaad.Infrastructure.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly ISecurityLogService _securityLog;
        private readonly AppDbContext _context;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtService jwtService,
            ISecurityLogService securityLog,
            AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _securityLog = securityLog;
            _context = context;
        }

        // ── Register ──────────────────────────────────────────────────────────
        public async Task<Result<AuthResponseDto>> RegisterAsync(
            RegisterDto dto,
            string ipAddress)
        {
            // تأكد إن الـ email مش موجود
            var existing = await _userManager.FindByEmailAsync(dto.Email);
            if (existing != null)
                return Result<AuthResponseDto>.Failure("Email already registered.");

            var user = new ApplicationUser
            {
                FullName = dto.FullName,
                Email = dto.Email,
                UserName = dto.Email, // Identity بتستخدم UserName للـ login
                JobTitle = dto.JobTitle,
                CompanyId = dto.CompanyId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return Result<AuthResponseDto>.Failure(
                    result.Errors.Select(e => e.Description));

            // الـ default role للـ user العادي
            await _userManager.AddToRoleAsync(user, "CompanyUser");

            await _securityLog.LogAsync(
                SecurityEventType.AccountRegistered,
                success: true,
                ipAddress: ipAddress,
                userId: user.Id,
                email: user.Email);

            return await BuildAuthResponseAsync(user, ipAddress);
        }

        // ── Login ─────────────────────────────────────────────────────────────
        public async Task<Result<AuthResponseDto>> LoginAsync(
            LoginDto dto,
            string ipAddress)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            // نرجع نفس الـ message عشان ما نكشفش إن الـ email موجود أو لأ
            if (user == null || !user.IsActive)
            {
                await _securityLog.LogAsync(
                    SecurityEventType.LoginFailed,
                    success: false,
                    ipAddress: ipAddress,
                    email: dto.Email,
                    details: user == null ? "User not found" : "Account inactive");

                return Result<AuthResponseDto>.Failure("Invalid email or password.");
            }

            // CheckPasswordSignInAsync بتتحكم في الـ lockout أوتوماتيك
            var signInResult = await _signInManager.CheckPasswordSignInAsync(
                user, dto.Password, lockoutOnFailure: true);

            if (signInResult.IsLockedOut)
            {
                await _securityLog.LogAsync(
                    SecurityEventType.AccountLockedOut,
                    success: false,
                    ipAddress: ipAddress,
                    userId: user.Id,
                    email: user.Email,
                    details: "Account locked out due to multiple failed attempts");

                return Result<AuthResponseDto>.Failure(
                    "Account locked. Try again later.");
            }

            if (signInResult.RequiresTwoFactor)
            {
                // بنرجع رسالة خاصة — الـ client يوجه لصفحة الـ 2FA
                return Result<AuthResponseDto>.Failure("2FA_REQUIRED");
            }

            if (!signInResult.Succeeded)
            {
                await _securityLog.LogAsync(
                    SecurityEventType.LoginFailed,
                    success: false,
                    ipAddress: ipAddress,
                    userId: user.Id,
                    email: user.Email,
                    details: "Invalid password");

                return Result<AuthResponseDto>.Failure("Invalid email or password.");
            }

            // تحديث آخر تسجيل دخول
            user.LastLoginDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            await _securityLog.LogAsync(
                SecurityEventType.LoginSuccess,
                success: true,
                ipAddress: ipAddress,
                userId: user.Id,
                email: user.Email);

            return await BuildAuthResponseAsync(user, ipAddress, dto.RememberMe);
        }

        // ── Refresh Token ─────────────────────────────────────────────────────
        public async Task<Result<AuthResponseDto>> RefreshTokenAsync(
            string refreshToken,
            string ipAddress)
        {
            var storedToken = await _context.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == refreshToken);

            if (storedToken == null)
                return Result<AuthResponseDto>.Failure("Invalid token.");

            // Reuse detection — لو token اتستخدم قبل كده
            // معناه في حد سرق الـ token فبنلغي كل tokens الـ user
            if (storedToken.IsRevoked)
            {
                await RevokeAllUserTokensAsync(
                    storedToken.UserId,
                    "Reuse detected — possible token theft",
                    ipAddress);

                await _securityLog.LogAsync(
                    SecurityEventType.TokenRevoked,
                    success: false,
                    ipAddress: ipAddress,
                    userId: storedToken.UserId,
                    details: "Refresh token reuse detected");

                return Result<AuthResponseDto>.Failure("Token reuse detected. Please login again.");
            }

            if (storedToken.IsExpired)
                return Result<AuthResponseDto>.Failure("Token expired.");

            var user = storedToken.User;

            // Rotate: نلغي القديم ونعمل جديد
            storedToken.IsRevoked = true;
            storedToken.RevokedByIp = ipAddress;
            storedToken.RevokedReason = "Rotated";

            await _securityLog.LogAsync(
                SecurityEventType.TokenRefreshed,
                success: true,
                ipAddress: ipAddress,
                userId: user.Id,
                email: user.Email);

            return await BuildAuthResponseAsync(user, ipAddress);
        }

        // ── Logout ────────────────────────────────────────────────────────────
        public async Task<Result> LogoutAsync(string refreshToken, string ipAddress)
        {
            var storedToken = await _context.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == refreshToken);

            if (storedToken == null || !storedToken.IsActive)
                return Result.Success(); // idempotent

            storedToken.IsRevoked = true;
            storedToken.RevokedByIp = ipAddress;
            storedToken.RevokedReason = "Logout";

            await _context.SaveChangesAsync();

            await _securityLog.LogAsync(
                SecurityEventType.Logout,
                success: true,
                ipAddress: ipAddress,
                userId: storedToken.UserId,
                email: storedToken.User?.Email);

            return Result.Success();
        }

        // ── Change Password ───────────────────────────────────────────────────
        public async Task<Result> ChangePasswordAsync(
            int userId,
            ChangePasswordDto dto,
            string ipAddress)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return Result.Failure("User not found.");

            var result = await _userManager.ChangePasswordAsync(
                user, dto.CurrentPassword, dto.NewPassword);

            if (!result.Succeeded)
            {
                await _securityLog.LogAsync(
                    SecurityEventType.PasswordChanged,
                    success: false,
                    ipAddress: ipAddress,
                    userId: userId,
                    details: "Wrong current password");

                return Result.Failure(result.Errors.Select(e => e.Description));
            }

            // نلغي كل الـ refresh tokens بعد تغيير الباسورد — security best practice
            await RevokeAllUserTokensAsync(userId, "Password changed", ipAddress);

            await _securityLog.LogAsync(
                SecurityEventType.PasswordChanged,
                success: true,
                ipAddress: ipAddress,
                userId: userId,
                email: user.Email);

            return Result.Success();
        }

        // ── Private Helpers ───────────────────────────────────────────────────

        private async Task<Result<AuthResponseDto>> BuildAuthResponseAsync(
            ApplicationUser user,
            string ipAddress,
            bool rememberMe = false)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = await _jwtService.GenerateAccessTokenAsync(user, roles);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // حفظ الـ refresh token في الـ DB
            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                CreatedByIp = ipAddress,
                // RememberMe → 30 يوم، عادي → 7 أيام
                ExpiresAt = DateTime.UtcNow.AddDays(rememberMe ? 30 : 7),
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            return Result<AuthResponseDto>.Success(new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiry = DateTime.UtcNow.AddMinutes(15),
                User = new UserInfoDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email!,
                    JobTitle = user.JobTitle,
                    CompanyId = user.CompanyId,
                    CompanyName = user.Company?.CompanyName,
                    Roles = roles,
                    TwoFactorEnabled = user.TwoFactorEnabled
                }
            });
        }

        private async Task RevokeAllUserTokensAsync(
            int userId,
            string reason,
            string ipAddress)
        {
            var tokens = await _context.RefreshTokens
                .Where(r => r.UserId == userId && !r.IsRevoked)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                token.RevokedByIp = ipAddress;
                token.RevokedReason = reason;
            }

            await _context.SaveChangesAsync();
        }
    }
}