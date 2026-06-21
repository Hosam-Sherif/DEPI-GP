// Mazaad.API/Controllers/TwoFactorController.cs

using Mazaad.Application.DTOs.Auth;
using Mazaad.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mazaad.API.Controllers
{
    [ApiController]
    [Route("api/2fa")]
    [Authorize]
    public class TwoFactorController : ControllerBase
    {
        private readonly ITwoFactorService _twoFactorService;

        public TwoFactorController(ITwoFactorService twoFactorService)
        {
            _twoFactorService = twoFactorService;
        }

        /// <summary>
        /// يجيب الـ QR Code لتفعيل الـ 2FA.
        /// الـ user يـ scan بـ Google Authenticator أو Authy.
        /// </summary>
        [HttpGet("setup")]
        public async Task<IActionResult> GetSetup()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var result = await _twoFactorService.GetSetupInfoAsync(userId.Value);

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            return Ok(result.Data);
        }

        /// <summary>
        /// تفعيل الـ 2FA بعد التحقق من الـ code.
        /// </summary>
        [HttpPost("enable")]
        public async Task<IActionResult> Enable([FromBody] TwoFactorToggleDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var result = await _twoFactorService.EnableAsync(
                userId.Value, dto, GetIpAddress());

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            return Ok(new { message = "Two-factor authentication enabled successfully." });
        }

        /// <summary>
        /// إلغاء الـ 2FA.
        /// </summary>
        [HttpPost("disable")]
        public async Task<IActionResult> Disable([FromBody] TwoFactorToggleDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var result = await _twoFactorService.DisableAsync(
                userId.Value, dto, GetIpAddress());

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            return Ok(new { message = "Two-factor authentication disabled." });
        }

        /// <summary>
        /// الـ Step 2 بعد الـ login لما الـ user عنده 2FA.
        /// مش محتاج [Authorize] لأن الـ user لسه مش logged in.
        /// </summary>
        [HttpPost("verify")]
        [AllowAnonymous]
        public async Task<IActionResult> Verify([FromBody] TwoFactorVerifyDto dto)
        {
            var result = await _twoFactorService.VerifyAndLoginAsync(dto, GetIpAddress());

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            return Ok(new
            {
                accessToken = result.Data!.AccessToken,
                accessTokenExpiry = result.Data.AccessTokenExpiry,
                user = result.Data.User
            });
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst("uid")?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }

        private string GetIpAddress() =>
            Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded)
                ? forwarded.ToString()
                : HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}