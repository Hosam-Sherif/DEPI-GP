// Mazaad.Infrastructure/Services/Auth/AuthService.cs

using Google.Apis.Auth;
using Mazaad.Application.Common;
using Mazaad.Application.DTOs.Auth;
using Mazaad.Application.Interfaces.Services;
using Mazaad.Domain.Enums;
using Mazaad.Domain.Models;
using Mazaad.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Mazaad.Infrastructure.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly ISecurityLogService _securityLog;
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtService jwtService,
            ISecurityLogService securityLog,
            AppDbContext context,
            IEmailService emailService,
            IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _securityLog = securityLog;
            _context = context;
            _emailService = emailService;
            _config = config;
        }

        // ── Register (مزايد عادي / Bidder) ──────────────────────────────────
        public async Task<Result<AuthResponseDto>> RegisterAsync(
            RegisterDto dto,
            string ipAddress)
        {
            var existing = await _userManager.FindByEmailAsync(dto.Email);
            if (existing != null)
                return Result<AuthResponseDto>.Failure("Email already registered.");

            var user = new ApplicationUser
            {
                FullName = dto.FullName,
                Email = dto.Email,
                UserName = dto.Email,
                JobTitle = dto.JobTitle,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return Result<AuthResponseDto>.Failure(
                    result.Errors.Select(e => e.Description));

            await _userManager.AddToRoleAsync(user, "Bidder");

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
                return Result<AuthResponseDto>.Failure("2FA_REQUIRED");

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

        // ── Google Login / Register ──────────────────────────────────────────
        public async Task<Result<GoogleAuthResponseDto>> GoogleLoginAsync(
            GoogleLoginDto dto,
            string ipAddress)
        {
            GoogleJsonWebSignature.Payload payload;
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _config["GoogleAuth:ClientId"] }
                };

                payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken, settings);
            }
            catch (Exception)
            {
                await _securityLog.LogAsync(
                    SecurityEventType.LoginFailed,
                    success: false,
                    ipAddress: ipAddress,
                    details: "Invalid Google ID token");

                return Result<GoogleAuthResponseDto>.Failure("Invalid Google token.");
            }

            var wantsCompanyAccount = string.Equals(
                dto.AccountType, "Company", StringComparison.OrdinalIgnoreCase);

            var user = await _userManager.FindByEmailAsync(payload.Email);
            var isNewUser = false;

            if (user == null)
            {
                user = new ApplicationUser
                {
                    FullName = payload.Name ?? payload.Email,
                    Email = payload.Email,
                    UserName = payload.Email,
                    EmailConfirmed = true,
                    IsActive = true,
                    ProfilePictureUrl = payload.Picture,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                    return Result<GoogleAuthResponseDto>.Failure(
                        createResult.Errors.Select(e => e.Description));

                await _userManager.AddToRoleAsync(
                    user, wantsCompanyAccount ? "CompanyAdmin" : "Bidder");

                isNewUser = true;
            }

            if (!user.IsActive)
                return Result<GoogleAuthResponseDto>.Failure("Account is inactive.");

            // نربط حساب جوجل بالمستخدم لو لسه مش مربوط (يفيد لو كان مسجل قبل كده بباسورد عادي)
            var logins = await _userManager.GetLoginsAsync(user);
            if (!logins.Any(l => l.LoginProvider == "Google" && l.ProviderKey == payload.Subject))
            {
                await _userManager.AddLoginAsync(
                    user, new UserLoginInfo("Google", payload.Subject, "Google"));
            }

            user.LastLoginDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            await _securityLog.LogAsync(
                isNewUser ? SecurityEventType.AccountRegistered : SecurityEventType.LoginSuccess,
                success: true,
                ipAddress: ipAddress,
                userId: user.Id,
                email: user.Email,
                details: "Google sign-in");

            var authResult = await BuildAuthResponseAsync(user, ipAddress);
            if (!authResult.Succeeded)
                return Result<GoogleAuthResponseDto>.Failure(authResult.Errors);

            var roles = authResult.Data!.User.Roles;
            var requiresCompanyCompletion =
                roles.Contains("CompanyAdmin") && user.CompanyId == null;

            return Result<GoogleAuthResponseDto>.Success(new GoogleAuthResponseDto
            {
                AccessToken = authResult.Data.AccessToken,
                RefreshToken = authResult.Data.RefreshToken,
                AccessTokenExpiry = authResult.Data.AccessTokenExpiry,
                User = authResult.Data.User,
                RequiresCompanyProfileCompletion = requiresCompanyCompletion
            });
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
                return Result.Success();

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

            await RevokeAllUserTokensAsync(userId, "Password changed", ipAddress);

            await _securityLog.LogAsync(
                SecurityEventType.PasswordChanged,
                success: true,
                ipAddress: ipAddress,
                userId: userId,
                email: user.Email);

            return Result.Success();
        }

        // ── Forgot Password ───────────────────────────────────────────────────
        /// <summary>
        /// بيرجع Success دايمًا (حتى لو الإيميل مش موجود) عشان منكشفش
        /// إن إيميل معين مسجل في النظام أو لأ (Email Enumeration Protection).
        /// </summary>
        public async Task<Result> ForgotPasswordAsync(
            ForgotPasswordDto dto,
            string ipAddress)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            // مفيش user بهذا الإيميل — نرجع Success برضو من غير ما نبعت حاجة
            if (user == null || !user.IsActive)
                return Result.Success();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var clientBaseUrl = _config["ClientApp:BaseUrl"] ?? "http://localhost:4200";
            var encodedToken = Uri.EscapeDataString(token);
            var encodedEmail = Uri.EscapeDataString(user.Email!);

            var resetLink =
                $"{clientBaseUrl}/reset-password?email={encodedEmail}&token={encodedToken}";

            var htmlBody = BuildResetPasswordEmailBody(user.FullName, resetLink);

            await _emailService.SendEmailAsync(
                user.Email!,
                "إعادة تعيين كلمة المرور — Mazzad",
                htmlBody);

            return Result.Success();
        }

        // ── Reset Password ────────────────────────────────────────────────────
        public async Task<Result> ResetPasswordAsync(
            ResetPasswordDto dto,
            string ipAddress)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Result.Failure("Invalid request.");

            var result = await _userManager.ResetPasswordAsync(
                user, dto.Token, dto.NewPassword);

            if (!result.Succeeded)
            {
                await _securityLog.LogAsync(
                    SecurityEventType.PasswordChanged,
                    success: false,
                    ipAddress: ipAddress,
                    userId: user.Id,
                    email: user.Email,
                    details: "Reset password failed — invalid or expired token");

                return Result.Failure(result.Errors.Select(e => e.Description));
            }

            // نلغي كل الـ refresh tokens — أي جلسة قديمة لازم تعمل login تاني
            await RevokeAllUserTokensAsync(user.Id, "Password reset via email link", ipAddress);

            await _securityLog.LogAsync(
                SecurityEventType.PasswordChanged,
                success: true,
                ipAddress: ipAddress,
                userId: user.Id,
                email: user.Email,
                details: "Reset via forgot-password flow");

            return Result.Success();
        }

        // ── Get My Profile ────────────────────────────────────────────────────
        public async Task<Result<MyProfileDto>> GetMyProfileAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return Result<MyProfileDto>.Failure("User not found.");

            return Result<MyProfileDto>.Success(new MyProfileDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                JobTitle = user.JobTitle,
                PhoneNumber = user.PhoneNumber,
                CompanyId = user.CompanyId,
                TwoFactorEnabled = user.TwoFactorEnabled,
                LastLoginDate = user.LastLoginDate,
                ProfilePictureUrl = user.ProfilePictureUrl
            });
        }

        // ── Update Profile ────────────────────────────────────────────────────
        public async Task<Result> UpdateProfileAsync(int userId, UpdateProfileDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return Result.Failure("User not found.");

            user.FullName = dto.FullName;
            user.JobTitle = dto.JobTitle ?? user.JobTitle;
            user.PhoneNumber = dto.PhoneNumber;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return Result.Failure(result.Errors.Select(e => e.Description));

            return Result.Success();
        }

        // ── Upload Profile Picture ────────────────────────────────────────────
        public async Task<Result<string>> UploadProfilePictureAsync(int userId, IFormFile file)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return Result<string>.Failure("User not found.");

            var uploadPath = Path.Combine("wwwroot", "uploads", "profiles");
            Directory.CreateDirectory(uploadPath);

            if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
            {
                var oldFilePath = Path.Combine("wwwroot", user.ProfilePictureUrl.TrimStart('/'));
                if (File.Exists(oldFilePath))
                    File.Delete(oldFilePath);
            }

            var extension = Path.GetExtension(file.FileName).ToLower();
            var fileName = $"profile_{userId}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativeUrl = $"/uploads/profiles/{fileName}";
            user.ProfilePictureUrl = relativeUrl;
            user.UpdatedAt = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);

            return Result<string>.Success(relativeUrl);
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

            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                CreatedByIp = ipAddress,
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

        private static string BuildResetPasswordEmailBody(string fullName, string resetLink)
        {
            return $@"
<div style=""font-family: 'Cairo', Tahoma, sans-serif; background:#0a0a0a; padding:32px; direction:rtl;"">
  <div style=""max-width:480px; margin:0 auto; background:#111111; border:1px solid rgba(201,168,76,0.25); border-radius:12px; padding:32px;"">
    <h2 style=""color:#c9a84c; margin-bottom:8px;"">إعادة تعيين كلمة المرور</h2>
    <p style=""color:#ffffff; font-size:14px; line-height:1.8;"">
      مرحبًا {fullName}،<br/>
      وصلنا طلب لإعادة تعيين كلمة مرور حسابك في Mazzad. اضغط على الزر بالأسفل لتعيين كلمة مرور جديدة.
    </p>
    <div style=""text-align:center; margin:28px 0;"">
      <a href=""{resetLink}""
         style=""display:inline-block; background:linear-gradient(135deg,#9a7a2e,#e8c97a); color:#0a0a0a; font-weight:700; padding:12px 32px; border-radius:6px; text-decoration:none;"">
        إعادة تعيين كلمة المرور
      </a>
    </div>
    <p style=""color:rgba(255,255,255,0.4); font-size:12px; line-height:1.7;"">
      هذا الرابط صالح لفترة محدودة. لو لم تطلب إعادة تعيين كلمة المرور، يمكنك تجاهل هذا البريد بأمان.
    </p>
  </div>
</div>";
        }
    }
}