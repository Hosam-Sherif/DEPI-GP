using System.Threading.Tasks;
using Mazaad.Application.DTOs;
using Mazaad.Application.Interfaces;
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

        /// <summary>Get all active industry types (used for marketplace sector filter).</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var industries = await _industryService.GetAllIndustriesAsync();
            return Ok(industries);
        }

        /// <summary>Get a single industry type by ID.</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var industry = await _industryService.GetIndustryByIdAsync(id);
            if (industry == null) return NotFound();
            return Ok(industry);
        }

        /// <summary>Create a new industry type.</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateIndustryDto request)
        {
            var created = await _industryService.CreateIndustryAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>Soft-delete an industry type.</summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _industryService.DeleteIndustryAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
