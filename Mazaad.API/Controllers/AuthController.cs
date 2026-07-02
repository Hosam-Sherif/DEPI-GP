// Mazaad.API/Controllers/AuthController.cs

using Mazaad.Application.DTOs.Auth;
using Mazaad.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Mazaad.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// تسجيل user جديد (مش مرتبط بشركة).
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto, GetIpAddress());

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            SetRefreshTokenCookie(result.Data!.RefreshToken);

            return Ok(new
            {
                accessToken = result.Data.AccessToken,
                accessTokenExpiry = result.Data.AccessTokenExpiry,
                user = result.Data.User
            });
        }

        /// <summary>
        /// تسجيل الدخول.
        /// لو الـ user عنده 2FA → بيرجع 2FA_REQUIRED.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto, GetIpAddress());

            if (!result.Succeeded)
            {
                if (result.Error == "2FA_REQUIRED")
                    return Ok(new { requiresTwoFactor = true, email = dto.Email });

                return BadRequest(new { errors = result.Errors });
            }

            SetRefreshTokenCookie(result.Data!.RefreshToken);

            return Ok(new
            {
                accessToken = result.Data.AccessToken,
                accessTokenExpiry = result.Data.AccessTokenExpiry,
                user = result.Data.User
            });
        }

        /// <summary>
        /// تسجيل الدخول / إنشاء حساب عبر جوجل.
        /// الفرونت بيبعت الـ ID Token اللي راجع من Google Identity Services
        /// + accountType ("Bidder" أو "Company") في حالة المستخدم جديد.
        /// </summary>
        [HttpPost("google-login")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto dto)
        {
            var result = await _authService.GoogleLoginAsync(dto, GetIpAddress());

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            SetRefreshTokenCookie(result.Data!.RefreshToken);

            return Ok(new
            {
                accessToken = result.Data.AccessToken,
                accessTokenExpiry = result.Data.AccessTokenExpiry,
                user = result.Data.User,
                requiresCompanyProfileCompletion = result.Data.RequiresCompanyProfileCompletion
            });
        }

        /// <summary>
        /// تجديد الـ Access Token باستخدام الـ Refresh Token من الـ Cookie.
        /// </summary>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized(new { error = "No refresh token provided." });

            var result = await _authService.RefreshTokenAsync(refreshToken, GetIpAddress());

            if (!result.Succeeded)
                return Unauthorized(new { errors = result.Errors });

            SetRefreshTokenCookie(result.Data!.RefreshToken);

            return Ok(new
            {
                accessToken = result.Data.AccessToken,
                accessTokenExpiry = result.Data.AccessTokenExpiry,
                user = result.Data.User
            });
        }

        /// <summary>
        /// تسجيل الخروج + إلغاء الـ Refresh Token.
        /// </summary>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (!string.IsNullOrEmpty(refreshToken))
                await _authService.LogoutAsync(refreshToken, GetIpAddress());

            Response.Cookies.Delete("refreshToken");

            return NoContent();
        }

        /// <summary>
        /// تغيير كلمة المرور — يحتاج login.
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var result = await _authService.ChangePasswordAsync(
                userId.Value, dto, GetIpAddress());

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            Response.Cookies.Delete("refreshToken");

            return NoContent();
        }

        /// <summary>
        /// طلب رابط استعادة كلمة المرور — بيتبعت دايمًا 200 OK
        /// (حتى لو الإيميل غير مسجل) عشان منكشفش وجود الإيميل من عدمه.
        /// </summary>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            await _authService.ForgotPasswordAsync(dto, GetIpAddress());

            return Ok(new
            {
                message = "لو الإيميل ده مسجل عندنا، تم إرسال رابط استعادة كلمة المرور إليه."
            });
        }

        /// <summary>
        /// تعيين كلمة مرور جديدة باستخدام الـ Token اللي جه في رابط الإيميل.
        /// </summary>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var result = await _authService.ResetPasswordAsync(dto, GetIpAddress());

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            return Ok(new { message = "تم تعيين كلمة المرور الجديدة بنجاح." });
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void SetRefreshTokenCookie(string token)
        {
            Response.Cookies.Append("refreshToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(30)
            });
        }

        private string GetIpAddress() =>
            Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded)
                ? forwarded.ToString()
                : HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst("uid")?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }
    }
}