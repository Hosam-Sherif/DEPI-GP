using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mazaad.Application.DTOs;
using Mazaad.Application.Interfaces;

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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var listings = await _listingService.GetAllListingsAsync();
            return Ok(listings);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var listing = await _listingService.GetListingByIdAsync(id);
            if (listing == null) return NotFound();
            return Ok(listing);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateListingDto request)
        {
            int currentCompanyId = 2; // Hardcoded for testing
            var createdListing = await _listingService.CreateListingAsync(currentCompanyId, request);
            return CreatedAtAction(nameof(GetById), new { id = createdListing.Id }, createdListing);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            int currentCompanyId = 2; // Hardcoded for testing
            var success = await _listingService.DeleteListingAsync(id, currentCompanyId);
            if (!success) return BadRequest("Could not delete listing. It may not exist or you don't own it.");
            return NoContent();
        }
    }
}