// Mazaad.API/Controllers/CompanyRegistrationController.cs

using Mazaad.Application.DTOs.Company;
using Mazaad.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mazaad.API.Controllers
{
    [ApiController]
    [Route("api/companies")]
    public class CompanyRegistrationController : ControllerBase
    {
        private readonly ICompanyRegistrationService _registrationService;
        private readonly ICompanyDocumentService _documentService;

        public CompanyRegistrationController(
            ICompanyRegistrationService registrationService,
            ICompanyDocumentService documentService)
        {
            _registrationService = registrationService;
            _documentService = documentService;
        }

        /// <summary>
        /// تسجيل شركة جديدة مع أول Admin ليها.
        /// [FromForm] عشان فيه file uploads.
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromForm] RegisterCompanyDto dto)
        {
            var result = await _registrationService.RegisterCompanyAsync(
                dto, GetIpAddress());

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            return StatusCode(201, new
            {
                message = "Company registered successfully. Pending admin verification.",
                accessToken = result.Data!.AccessToken,
                accessTokenExpiry = result.Data.AccessTokenExpiry,
                user = result.Data.User
            });
        }

        /// <summary>
        /// يجيب الشركات المنتظرة — SuperAdmin فقط.
        /// </summary>
        [HttpGet("pending")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetPending()
        {
            var companies = await _registrationService.GetPendingCompaniesAsync();
            return Ok(companies);
        }

        /// <summary>
        /// الموافقة أو الرفض — SuperAdmin فقط.
        /// </summary>
        [HttpPatch("{id}/verify")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Verify(int id, [FromBody] VerifyCompanyDto dto)
        {
            var adminUserId = GetCurrentUserId();
            if (adminUserId == null) return Unauthorized();

            var result = await _registrationService.VerifyCompanyAsync(
                id, adminUserId.Value, dto, GetIpAddress());

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            return NoContent();
        }

        /// <summary>
        /// يجيب مستندات شركة — SuperAdmin فقط.
        /// </summary>
        [HttpGet("{id}/documents")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetDocuments(int id)
        {
            var documents = await _documentService.GetCompanyDocumentsAsync(id);
            return Ok(documents);
        }

        /// <summary>
        /// تحميل مستند — SuperAdmin فقط.
        /// </summary>
        [HttpGet("documents/{documentId}/download")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DownloadDocument(int documentId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var result = await _documentService.DownloadAsync(documentId, userId.Value);

            if (!result.Succeeded)
                return NotFound(new { error = result.Error });

            var (fileStream, contentType, fileName) = result.Data;
            return File(fileStream, contentType, fileName);
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