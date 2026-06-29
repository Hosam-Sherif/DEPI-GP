using Mazaad.Application.DTOs.CommissionPolicies;
using Mazaad.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mazaad.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin")]
    public class CommissionPoliciesController : ControllerBase
    {
        private readonly ICommissionPolicyService _service;

        public CommissionPoliciesController(ICommissionPolicyService service)
        {
            _service = service;
        }

        /// <summary>
        /// Returns all commission policies ordered by EffectiveFrom descending.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CommissionPolicyDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(data);
        }

        /// <summary>
        /// Returns a single commission policy by ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(CommissionPolicyDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var policy = await _service.GetByIdAsync(id);
            if (policy is null)
                return NotFound(new { message = $"Commission policy with id {id} was not found." });

            return Ok(policy);
        }

        /// <summary>
        /// Creates a new commission policy (Active = true by default).
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(CommissionPolicyDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateCommissionPolicyDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var created = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing commission policy (does NOT change Active flag).
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCommissionPolicyDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updated = await _service.UpdateAsync(id, dto);
                if (!updated)
                    return NotFound(new { message = $"Commission policy with id {id} was not found." });

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Soft-deactivates a policy (Active → false). Cannot be reversed via API.
        /// </summary>
        [HttpPatch("{id:int}/deactivate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Deactivate(int id)
        {
            var result = await _service.DeactivateAsync(id);

            if (!result)
                return NotFound(new { message = $"Commission policy with id {id} was not found or is already inactive." });

            return NoContent();
        }
    }
}