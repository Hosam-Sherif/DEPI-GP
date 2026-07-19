// Mazaad.Infrastructure/Services/Auth/TwoFactorService.cs

using System.Text;
using System.Text.Encodings.Web;
using Mazaad.Application.Common;
using Mazaad.Application.DTOs.Auth;
using Mazaad.Application.Interfaces.Services;
using Mazaad.Domain.Enums;
using Mazaad.Domain.Models;
using Microsoft.AspNetCore.Identity;
using QRCoder;

namespace Mazaad.Infrastructure.Services.Auth
{
    public class TwoFactorService : ITwoFactorService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtService _jwtService;
        private readonly ISecurityLogService _securityLog;

        public TwoFactorService(
            UserManager<ApplicationUser> userManager,
            IJwtService jwtService,
            ISecurityLogService securityLog)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _securityLog = securityLog;
        }

        // ── Get Setup Info (QR Code) ──────────────────────────────────────────
        public async Task<Result<TwoFactorSetupDto>> GetSetupInfoAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return Result<TwoFactorSetupDto>.Failure("User not found.");

            // Identity بتولد الـ authenticator key أوتوماتيك وتحفظه
            await _userManager.ResetAuthenticatorKeyAsync(user);
            var key = await _userManager.GetAuthenticatorKeyAsync(user);

            // الـ format اللي الـ authenticator apps بتفهمه
            var otpAuthUri =
                $"otpauth://totp/Mazaad:{UrlEncoder.Default.Encode(user.Email!)}?secret={key}&issuer=Mazaad&digits=6";

            // نولد QR Code كـ PNG بـ pure .NET (مش محتاجين library خارجية)
            var qrCodeBase64 = GenerateQrCodeBase64(otpAuthUri);

            return Result<TwoFactorSetupDto>.Success(new TwoFactorSetupDto
            {
                QrCodeBase64 = qrCodeBase64,
                ManualEntryKey = FormatKey(key!)
            });
        }

        // ── Enable 2FA ────────────────────────────────────────────────────────
        public async Task<Result> EnableAsync(
            int userId,
            TwoFactorToggleDto dto,
            string ipAddress)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return Result.Failure("User not found.");

            // نتحقق من الـ code قبل تفعيل الـ 2FA
            var isValid = await _userManager.VerifyTwoFactorTokenAsync(
                user,
                _userManager.Options.Tokens.AuthenticatorTokenProvider,
                dto.VerificationCode);

            if (!isValid)
            {
                await _securityLog.LogAsync(
                    SecurityEventType.TwoFactorLoginFailed,
                    success: false,
                    ipAddress: ipAddress,
                    userId: userId,
                    details: "Invalid 2FA code during enable");

                return Result.Failure("Invalid verification code.");
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);

            await _securityLog.LogAsync(
                SecurityEventType.TwoFactorEnabled,
                success: true,
                ipAddress: ipAddress,
                userId: userId,
                email: user.Email);

            return Result.Success();
        }

        // ── Disable 2FA ───────────────────────────────────────────────────────
        public async Task<Result> DisableAsync(
            int userId,
            TwoFactorToggleDto dto,
            string ipAddress)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return Result.Failure("User not found.");

            var isValid = await _userManager.VerifyTwoFactorTokenAsync(
                user,
                _userManager.Options.Tokens.AuthenticatorTokenProvider,
                dto.VerificationCode);

            if (!isValid)
                return Result.Failure("Invalid verification code.");

            await _userManager.SetTwoFactorEnabledAsync(user, false);
            await _userManager.ResetAuthenticatorKeyAsync(user);

            await _securityLog.LogAsync(
                SecurityEventType.TwoFactorDisabled,
                success: true,
                ipAddress: ipAddress,
                userId: userId,
                email: user.Email);

            return Result.Success();
        }

        // ── Verify & Login (2FA step 2) ───────────────────────────────────────
        public async Task<Result<AuthResponseDto>> VerifyAndLoginAsync(
            TwoFactorVerifyDto dto,
            string ipAddress)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Result<AuthResponseDto>.Failure("Invalid request.");

            var isValid = await _userManager.VerifyTwoFactorTokenAsync(
                user,
                _userManager.Options.Tokens.AuthenticatorTokenProvider,
                dto.Code);

            if (!isValid)
            {
                await _securityLog.LogAsync(
                    SecurityEventType.TwoFactorLoginFailed,
                    success: false,
                    ipAddress: ipAddress,
                    userId: user.Id,
                    email: user.Email,
                    details: "Invalid 2FA code");

                return Result<AuthResponseDto>.Failure("Invalid 2FA code.");
            }

            user.LastLoginDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            await _securityLog.LogAsync(
                SecurityEventType.TwoFactorLoginSuccess,
                success: true,
                ipAddress: ipAddress,
                userId: user.Id,
                email: user.Email);

            // نبني الـ response نفسه زي الـ normal login
            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = await _jwtService.GenerateAccessTokenAsync(user, roles);

            return Result<AuthResponseDto>.Success(new AuthResponseDto
            {
                AccessToken = accessToken,
                AccessTokenExpiry = DateTime.UtcNow.AddMinutes(10080),
                User = new UserInfoDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email!,
                    JobTitle = user.JobTitle,
                    CompanyId = user.CompanyId,
                    Roles = roles,
                    TwoFactorEnabled = true
                }
            });
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        // بيقسم الـ key لمجموعات من 4 أحرف للقراءة السهلة
        private static string FormatKey(string key)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < key.Length)
            {
                result.Append(key.AsSpan(currentPosition, 4)).Append(' ');
                currentPosition += 4;
            }
            if (currentPosition < key.Length)
                result.Append(key.AsSpan(currentPosition));

            return result.ToString().ToLowerInvariant();
        }

        private static string GenerateQrCodeBase64(string content)
        {
            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeBytes = qrCode.GetGraphic(20);
            return Convert.ToBase64String(qrCodeBytes);
        }
    }
}