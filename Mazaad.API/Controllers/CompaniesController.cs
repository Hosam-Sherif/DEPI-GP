using System.Threading.Tasks;
using Mazaad.Application.DTOs;
using Mazaad.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Mazaad.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        public CompaniesController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        /// <summary>Get all companies.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var companies = await _companyService.GetAllCompaniesAsync();
            return Ok(companies);
        }

        /// <summary>Get a single company by ID.</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var company = await _companyService.GetCompanyByIdAsync(id);
            if (company == null) return NotFound();
            return Ok(company);
        }

        /// <summary>Register a new company.</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCompanyDto request)
        {
            var created = await _companyService.CreateCompanyAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>Verify / approve a company (admin action).</summary>
        [HttpPatch("{id}/verify")]
        public async Task<IActionResult> Verify(int id)
        {
            var success = await _companyService.VerifyCompanyAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
