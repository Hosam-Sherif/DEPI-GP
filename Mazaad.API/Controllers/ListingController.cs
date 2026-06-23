using System.Security.Claims;
using Mazaad.Application.DTOs;
using Mazaad.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mazaad.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ListingController : ControllerBase
    {
        private readonly IListingService _listingService;

        public ListingController(IListingService listingService)
        {
            _listingService = listingService;
        }

        /// <summary>
        /// Marketplace grid: returns paginated listing cards with optional filters.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ListingFilterDto filter)
        {
            var result = await _listingService.GetListingsAsync(filter);
            return Ok(result);
        }

        /// <summary>Summary of a single listing.</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var listing = await _listingService.GetListingByIdAsync(id);
            if (listing == null) return NotFound();
            return Ok(listing);
        }

        /// <summary>
        /// Full bidding-room detail: includes company name, category name,
        /// technical specs, location, due-diligence URLs, and top 5 bids.
        /// </summary>
        [HttpGet("{id}/detail")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var detail = await _listingService.GetListingDetailAsync(id);
            if (detail == null) return NotFound();
            return Ok(detail);
        }

        /// <summary>Create a new listing. Requires CompanyAdmin or CompanyUser JWT token.</summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateListingDto request)
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Unauthorized(new { error = "A valid company account is required to create listings." });

            var created = await _listingService.CreateListingAsync(companyId.Value, request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>Update a listing's mutable fields. Only members of the owning company can update.</summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] CreateListingDto request)
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Unauthorized(new { error = "A valid company account is required to update listings." });

            var updated = await _listingService.UpdateListingAsync(id, companyId.Value, request);
            if (updated == null) return NotFound(new { error = "Listing not found or you do not own it." });
            return Ok(updated);
        }

        /// <summary>Soft-delete a listing. Only members of the owning company can delete.</summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Unauthorized(new { error = "A valid company account is required to delete listings." });

            var success = await _listingService.DeleteListingAsync(id, companyId.Value);
            if (!success) return BadRequest(new { error = "Could not delete listing. It may not exist or you don't own it." });
            return NoContent();
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        /// <summary>Extract companyId from JWT claims. Returns null if not present or not a valid company user.</summary>
        private int? GetCurrentCompanyId()
        {
            var claim = User.FindFirst("companyId")?.Value;
            if (string.IsNullOrWhiteSpace(claim)) return null;
            return int.TryParse(claim, out var id) && id > 0 ? id : null;
        }
    }
}