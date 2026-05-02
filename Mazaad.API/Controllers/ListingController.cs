using System.Threading.Tasks;
using Mazaad.Application.DTOs;
using Mazaad.Application.Interfaces;
using Mazaad.Domain.Enums;
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
        /// Matches the left-panel filters shown in the Institutional Marketplace screen.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ListingFilterDto filter)
        {
            var result = await _listingService.GetListingsAsync(filter);
            return Ok(result);
        }

        /// <summary>Summary of a single listing (backward-compatible endpoint).</summary>
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

        /// <summary>Create a new listing (seller creates the auction).</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateListingDto request)
        {
            // TODO: replace with JWT claim extraction
            int currentCompanyId = 1;
            var created = await _listingService.CreateListingAsync(currentCompanyId, request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>Update a listing's mutable fields (title, dates, price, etc.).</summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateListingDto request)
        {
            int currentCompanyId = 1;
            var updated = await _listingService.UpdateListingAsync(id, currentCompanyId, request);
            if (updated == null) return NotFound("Listing not found or you do not own it.");
            return Ok(updated);
        }

        /// <summary>Soft-delete a listing (marks IsDeleted = true).</summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            int currentCompanyId = 1;
            var success = await _listingService.DeleteListingAsync(id, currentCompanyId);
            if (!success) return BadRequest("Could not delete listing. It may not exist or you don't own it.");
            return NoContent();
        }
    }
}