using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mazaad.Application.DTOs;
using Mazaad.Application.Interfaces;

namespace Mazaad.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BiddingController : ControllerBase
    {
        private readonly IBiddingService _biddingService;

        public BiddingController(IBiddingService biddingService)
        {
            _biddingService = biddingService;
        }

        [HttpPost("place-bid")]
        public async Task<IActionResult> PlaceBid([FromBody] PlaceBidDto request)
        {
            // For testing purposes, we are hardcoding the User ID and Company ID.
            // In a real application, you would extract these from the user's secure JWT token.
            int currentUserId = 1;
            int currentCompanyId = 2; // Company 2 = Buyer Co (seeded)

            var result = await _biddingService.PlaceBidAsync(currentUserId, currentCompanyId, request);

            if (!result.Success)
            {
                // Returns an HTTP 400 Bad Request with your error message
                return BadRequest(result);
            }

            // Returns an HTTP 200 OK with the success message and new price
            return Ok(result);
        }
        [HttpGet("listing/{listingId}")]
        public async Task<IActionResult> GetBidsForListing(int listingId)
        {
            var bids = await _biddingService.GetBidsForListingAsync(listingId);
            return Ok(bids);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBid(int id)
        {
            int currentCompanyId = 2; // Company 2 = Buyer Co (seeded)
            var success = await _biddingService.DeleteBidAsync(id, currentCompanyId);

            if (!success) return BadRequest("Could not cancel bid.");
            return NoContent();
        }
    }
}