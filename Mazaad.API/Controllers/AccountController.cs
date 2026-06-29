// Mazaad.API/Controllers/AccountController.cs

using Mazaad.Application.DTOs.Auth;
using Mazaad.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mazaad.API.Controllers
{
    [ApiController]
    [Route("api/account")]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// جلب بيانات الـ profile للـ user الحالي.
        /// GET /api/account/profile
        /// </summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var result = await _authService.GetMyProfileAsync(userId.Value);

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            return Ok(result.Data);
        }

        /// <summary>
        /// تعديل بيانات الـ profile (الاسم، المسمى الوظيفي، الهاتف).
        /// PUT /api/account/profile
        /// </summary>
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var result = await _authService.UpdateProfileAsync(userId.Value, dto);

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            return NoContent();
        }

        /// <summary>
        /// رفع صورة شخصية جديدة.
        /// POST /api/account/profile/picture
        /// Content-Type: multipart/form-data
        /// Field name: file
        /// </summary>
        [HttpPost("profile/picture")]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "لم يتم رفع أي ملف." });

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                return BadRequest(new { error = "نوع الملف غير مسموح. يُسمح فقط بـ JPG أو PNG أو WEBP." });

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new { error = "حجم الملف كبير جداً. الحد الأقصى 5MB." });

            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var result = await _authService.UploadProfilePictureAsync(userId.Value, file);

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            return Ok(new { profilePictureUrl = result.Data });
        }

        // ── Helper ────────────────────────────────────────────────────────────

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst("uid")?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }
    }
}