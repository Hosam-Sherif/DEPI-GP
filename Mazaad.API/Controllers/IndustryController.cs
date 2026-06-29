using System.Threading.Tasks;
using Mazaad.Application.DTOs;
using Mazaad.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mazaad.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IndustryController : ControllerBase
    {
        private readonly IIndustryService _industryService;

        public IndustryController(IIndustryService industryService)
        {
            _industryService = industryService;
        }

        /// <summary>
        /// Get all active industry types — PUBLIC.
        /// Used for marketplace sector filter + company registration dropdown.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var industries = await _industryService.GetAllIndustriesAsync();
            return Ok(industries);
        }

        /// <summary>Get a single industry type by ID — PUBLIC.</summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var industry = await _industryService.GetIndustryByIdAsync(id);
            if (industry == null) return NotFound();
            return Ok(industry);
        }

        /// <summary>Create a new industry type — SuperAdmin only.</summary>
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Create([FromBody] CreateIndustryDto request)
        {
            var created = await _industryService.CreateIndustryAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>Update an industry type's name — SuperAdmin only.</summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateIndustryDto request)
        {
            var updated = await _industryService.UpdateIndustryAsync(id, request);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        /// <summary>Soft-delete an industry type — SuperAdmin only.</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _industryService.DeleteIndustryAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}