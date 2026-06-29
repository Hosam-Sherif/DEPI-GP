using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Mazaad.Application.DTOs;
using Mazaad.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mazaad.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        public CompaniesController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        /// <summary>
        /// الشركات الموثقة — Public، لا يحتاج login.
        /// GET /api/companies/verified
        /// </summary>
        [HttpGet("verified")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVerified()
        {
            var companies = await _companyService.GetVerifiedCompaniesAsync();
            return Ok(companies);
        }

        /// <summary>Get all companies — SuperAdmin only.</summary>
        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetAll()
        {
            var companies = await _companyService.GetAllCompaniesAsync();
            return Ok(companies);
        }

        /// <summary>Get companies pending approval — SuperAdmin only.</summary>
        [HttpGet("pending")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetPending()
        {
            var companies = await _companyService.GetPendingCompaniesAsync();
            return Ok(companies);
        }

        /// <summary>Get a single company by ID — CompanyAdmin/CompanyUser/SuperAdmin.</summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin,CompanyAdmin,CompanyUser")]
        public async Task<IActionResult> GetById(int id)
        {
            var company = await _companyService.GetCompanyByIdAsync(id);
            if (company == null) return NotFound();
            return Ok(company);
        }

        /// <summary>Verify / approve a company — SuperAdmin only.</summary>
        [HttpPatch("{id}/verify")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Verify(int id)
        {
            var success = await _companyService.VerifyCompanyAsync(id, GetCurrentUserId());
            if (!success) return NotFound();
            return NoContent();
        }

        /// <summary>Reject a company with a reason — SuperAdmin only.</summary>
        [HttpPatch("{id}/reject")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectCompanyDto request)
        {
            var success = await _companyService.RejectCompanyAsync(id, request.Reason, GetCurrentUserId());
            if (!success) return NotFound();
            return NoContent();
        }

        // ── Helper ────────────────────────────────────────────────────────────
        private int GetCurrentUserId()
        {
            var claim = User.FindFirst("uid")?.Value
                     ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }
    }
}