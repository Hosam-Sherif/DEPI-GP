using Mazaad.Application.DTOs.Company;
using Mazaad.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mazaad.API.Controllers
{
    [ApiController]
    [Route("api/companies")]
    [Authorize(Roles = "SuperAdmin")]
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
            var result = await _registrationService.RegisterCompanyAsync(dto, GetIpAddress());

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            return StatusCode(201, new
            {
                message = "Company registered successfully. Your account will be activated once a SuperAdmin verifies your company documents. Please try logging in after verification.",
                companyId = result.Data!.CompanyId,
                status = result.Data.Status
            });
        }

        /// <summary>
        /// يجيب مستندات شركة — SuperAdmin فقط.
        /// </summary>
        [HttpGet("{id}/documents")]
        public async Task<IActionResult> GetDocuments(int id)
        {
            var documents = await _documentService.GetCompanyDocumentsAsync(id);
            return Ok(documents);
        }

        /// <summary>
        /// تحميل مستند — SuperAdmin فقط.
        /// </summary>
        [HttpGet("documents/{documentId}/download")]
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