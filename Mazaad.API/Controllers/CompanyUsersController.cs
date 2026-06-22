// Mazaad.API/Controllers/CompanyUsersController.cs

using Mazaad.Application.DTOs.Company;
using Mazaad.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mazaad.API.Controllers
{
    [ApiController]
    [Route("api/companies/{companyId}/users")]
    //[Authorize(Roles = "CompanyAdmin,SuperAdmin")]
    public class CompanyUsersController : ControllerBase
    {
        private readonly ICompanyUserService _companyUserService;

        public CompanyUsersController(ICompanyUserService companyUserService)
        {
            _companyUserService = companyUserService;
        }

        /// <summary>
        /// يجيب كل users الشركة.
        /// CompanyAdmin يشوف شركته بس — SuperAdmin يشوف أي شركة.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUsers(int companyId)
        {
            if (!CanAccessCompany(companyId))
                return Forbid();

            var users = await _companyUserService.GetUsersAsync(companyId);
            return Ok(users);
        }

        /// <summary>
        /// إضافة user جديد للشركة.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddUser(
            int companyId,
            [FromBody] AddCompanyUserDto dto)
        {
            if (!CanAccessCompany(companyId))
                return Forbid();

            var result = await _companyUserService.AddUserAsync(
                companyId, dto, GetIpAddress());

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            return StatusCode(201, result.Data);
        }

        /// <summary>
        /// تعديل role أو تفعيل/تعطيل user.
        /// </summary>
        [HttpPatch("{userId}")]
        public async Task<IActionResult> UpdateUser(
            int companyId,
            int userId,
            [FromBody] UpdateCompanyUserDto dto)
        {
            if (!CanAccessCompany(companyId))
                return Forbid();

            var result = await _companyUserService.UpdateUserAsync(
                companyId, userId, dto, GetIpAddress());

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            return NoContent();
        }

        /// <summary>
        /// إزالة user من الشركة (soft delete).
        /// </summary>
        [HttpDelete("{userId}")]
        public async Task<IActionResult> RemoveUser(int companyId, int userId)
        {
            if (!CanAccessCompany(companyId))
                return Forbid();

            var result = await _companyUserService.RemoveUserAsync(
                companyId, userId, GetIpAddress());

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            return NoContent();
        }

        /// <summary>
        /// الحصول على بيانات يوزر معين داخل الشركة.
        /// </summary>
        
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById(int companyId, int userId)
        {
            if (!CanAccessCompany(companyId))
                return Forbid();

            var user = await _companyUserService.GetUserByIdAsync(companyId, userId);

            if (user == null)
                return NotFound(new { message = "User not found in this company." });

            return Ok(user);
        }
        
        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>
        /// CompanyAdmin يقدر يوصل لشركته بس.
        /// SuperAdmin يقدر يوصل لأي شركة.
        /// </summary>
        private bool CanAccessCompany(int companyId)
        {
            if (User.IsInRole("SuperAdmin"))
                return true;

            var userCompanyId = User.FindFirst("companyId")?.Value;
            return userCompanyId == companyId.ToString();
        }

        private string GetIpAddress() =>
            Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded)
                ? forwarded.ToString()
                : HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

}