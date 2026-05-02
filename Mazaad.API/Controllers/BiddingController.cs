using System.Threading.Tasks;
using Mazaad.Application.DTOs;
using Mazaad.Application.Interfaces;
using Mazaad.API.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Mazaad.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BiddingController : ControllerBase
    {
        private readonly IBiddingService _biddingService;
        private readonly IHubContext<BiddingHub> _hubContext;

        public BiddingController(IBiddingService biddingService, IHubContext<BiddingHub> hubContext)
        {
            _biddingService = biddingService;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Place a full bid from the bidding room execution panel.
        /// After success, broadcasts a real-time update to all listeners on the listing's SignalR group.
        /// </summary>
        [HttpPost("place-bid")]
        public async Task<IActionResult> PlaceBid([FromBody] PlaceBidDto request)
        {
            // TODO: extract from JWT
            int currentUserId = 1;
            int currentCompanyId = 2;

            var result = await _biddingService.PlaceBidAsync(currentUserId, currentCompanyId, request);

            if (!result.Success)
                return BadRequest(result);

            // Broadcast real-time update to the auction room group
            var liveUpdate = new LiveBidUpdateDto
            {
                ListingId = request.ListingId,
                BidId = result.NewBidId,
                DisplayBidderName = result.DisplayBiddersName,
                NewHighestBid = result.NewPrice,
                TotalBidCount = result.NewBidCount,
                Timestamp = System.DateTime.UtcNow
            };
            await _hubContext.Clients.Group($"listing-{request.ListingId}")
                .SendAsync("BidPlaced", liveUpdate);

            return Ok(result);
        }

        /// <summary>
        /// Quick bid from a marketplace card — automatically uses the listing's minimum order quantity.
        /// </summary>
        [HttpPost("quick-bid")]
        public async Task<IActionResult> QuickBid([FromBody] QuickBidDto request)
        {
            int currentUserId = 1;
            int currentCompanyId = 2;

            var result = await _biddingService.PlaceQuickBidAsync(currentUserId, currentCompanyId, request);

            if (!result.Success)
                return BadRequest(result);

            // Broadcast to listing group
            var liveUpdate = new LiveBidUpdateDto
            {
                ListingId = request.ListingId,
                BidId = result.NewBidId,
                DisplayBidderName = result.DisplayBiddersName,
                NewHighestBid = result.NewPrice,
                TotalBidCount = result.NewBidCount,
                Timestamp = System.DateTime.UtcNow
            };
            await _hubContext.Clients.Group($"listing-{request.ListingId}")
                .SendAsync("BidPlaced", liveUpdate);

            return Ok(result);
        }

        /// <summary>All bids for a listing, ordered by amount descending (basic summary).</summary>
        [HttpGet("listing/{listingId}")]
        public async Task<IActionResult> GetBidsForListing(int listingId)
        {
            var bids = await _biddingService.GetBidsForListingAsync(listingId);
            return Ok(bids);
        }

        /// <summary>
        /// Live state for a listing: top 10 bids with full details for the bidding room feed.
        /// Also used to initialize the SignalR client state on page load.
        /// </summary>
        [HttpGet("listing/{listingId}/live")]
        public async Task<IActionResult> GetLiveBids(int listingId)
        {
            var bids = await _biddingService.GetLiveBidsAsync(listingId);
            return Ok(bids);
        }

        /// <summary>Get the full detail of a single bid.</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBidDetail(int id)
        {
            var bid = await _biddingService.GetBidDetailAsync(id);
            if (bid == null) return NotFound();
            return Ok(bid);
        }

        /// <summary>Cancel / soft-delete a bid (marks status = Cancelled).</summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBid(int id)
        {
            int currentCompanyId = 2;
            var success = await _biddingService.DeleteBidAsync(id, currentCompanyId);

            if (!success) return BadRequest("Could not cancel bid.");
            return NoContent();
        }
    }
}