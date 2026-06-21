// Mazaad.API/Controllers/SecurityLogsController.cs

using Mazaad.Application.Interfaces.Services;
using Mazaad.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mazaad.API.Controllers
{
    [ApiController]
    [Route("api/security-logs")]
    [Authorize]
    public class SecurityLogsController : ControllerBase
    {
        private readonly ISecurityLogService _securityLogService;

        public SecurityLogsController(ISecurityLogService securityLogService)
        {
            _securityLogService = securityLogService;
        }

        /// <summary>
        /// الـ user يشوف سجلاته الخاصة.
        /// </summary>
        [HttpGet("my")]
        public async Task<IActionResult> GetMyLogs([FromQuery] int count = 50)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var logs = await _securityLogService.GetUserLogsAsync(userId.Value, count);
            return Ok(logs);
        }

        /// <summary>
        /// SuperAdmin يشوف كل السجلات مع فلترة.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetAll(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] SecurityEventType? eventType,
            [FromQuery] int count = 100)
        {
            var logs = await _securityLogService.GetAllLogsAsync(
                from, to, eventType, count);

            return Ok(logs);
        }

        /// <summary>
        /// SuperAdmin يشوف سجلات user معين.
        /// </summary>
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetUserLogs(
            int userId,
            [FromQuery] int count = 50)
        {
            var logs = await _securityLogService.GetUserLogsAsync(userId, count);
            return Ok(logs);
        }

        // ── Helper ────────────────────────────────────────────────────────────

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst("uid")?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }
    }
}