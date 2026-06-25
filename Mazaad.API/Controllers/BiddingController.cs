using Mazaad.API.Hubs;
using Mazaad.Application.DTOs;
using Mazaad.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Mazaad.API.Controllers
{
    [ApiController]
    [Route("api/bidding")]
    [Authorize]
    public class BiddingController : ControllerBase
    {
        private readonly IBiddingService _biddingService;
        private readonly IHubContext<AuctionHub> _hubContext;
        private readonly ILogger<BiddingController> _logger;

        public BiddingController(
            IBiddingService biddingService,
            IHubContext<AuctionHub> hubContext,
            ILogger<BiddingController> logger)
        {
            _biddingService = biddingService;
            _hubContext = hubContext;
            _logger = logger;
        }

        #region PLACE BID

        /// <summary>
        /// Place secure auction bid
        /// </summary>
        [HttpPost("place-bid")]
        public async Task<IActionResult> PlaceBid(
            [FromBody] PlaceBidDto request)
        {
            try
            {
                if (!int.TryParse(User.FindFirst("uid")?.Value, out int userId) || userId <= 0)
                    return Unauthorized(new { success = false, message = "Invalid user token." });

                if (!int.TryParse(User.FindFirst("companyId")?.Value, out int companyId) || companyId <= 0)
                    return Unauthorized(new { success = false, message = "Only verified company users can place bids." });

                var result =
                    await _biddingService.PlaceBidAsync(
                        userId,
                        companyId,
                        request);

                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = result.Message
                    });
                }

                // Realtime broadcast
                var liveUpdate = new LiveBidUpdateDto
                {
                    ListingId = request.ListingId,
                    BidId = result.NewBidId,
                    DisplayBidderName = result.DisplayBiddersName,
                    NewHighestBid = result.NewPrice,
                    TotalBidCount = result.NewBidCount,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients
                    .Group($"auction-{request.ListingId}")
                    .SendAsync("BidPlaced", liveUpdate);

                // Also notify BiddingHub clients (group: listing-{id})
                await _hubContext.Clients
                    .Group($"listing-{request.ListingId}")
                    .SendAsync("BidPlaced", liveUpdate);

                _logger.LogInformation(
                    "Bid placed successfully on listing {ListingId}",
                    request.ListingId);

                return Ok(new
                {
                    success = true,
                    message = "Bid placed successfully.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while placing bid");

                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error."
                });
            }
        }

        #endregion

        #region QUICK BID

        /// <summary>
        /// Quick predefined increment bid
        /// </summary>
        [HttpPost("quick-bid")]
        public async Task<IActionResult> QuickBid(
            [FromBody] QuickBidDto request)
        {
            try
            {
                if (!int.TryParse(User.FindFirst("uid")?.Value, out int userId) || userId <= 0)
                    return Unauthorized(new { success = false, message = "Invalid user token." });

                if (!int.TryParse(User.FindFirst("companyId")?.Value, out int companyId) || companyId <= 0)
                    return Unauthorized(new { success = false, message = "Only verified company users can place quick bids." });

                var result =
                    await _biddingService.PlaceQuickBidAsync(
                        userId,
                        companyId,
                        request);

                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = result.Message
                    });
                }

                var liveUpdate = new LiveBidUpdateDto
                {
                    ListingId = request.ListingId,
                    BidId = result.NewBidId,
                    DisplayBidderName = result.DisplayBiddersName,
                    NewHighestBid = result.NewPrice,
                    TotalBidCount = result.NewBidCount,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients
                    .Group($"auction-{request.ListingId}")
                    .SendAsync("BidPlaced", liveUpdate);

                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while placing quick bid");

                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error."
                });
            }
        }

        #endregion

        #region GET BIDS

        /// <summary>
        /// Get all bids for listing — public, no auth required.
        /// </summary>
        [HttpGet("listing/{listingId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetListingBids(int listingId)
        {
            var bids =
                await _biddingService
                    .GetBidsForListingAsync(listingId);

            return Ok(bids);
        }

        /// <summary>
        /// Get live auction state — public, no auth required.
        /// </summary>
        [HttpGet("listing/{listingId}/live")]
        [AllowAnonymous]
        public async Task<IActionResult> GetLiveBids(int listingId)
        {
            var liveBids =
                await _biddingService
                    .GetLiveBidsAsync(listingId);

            return Ok(liveBids);
        }

        /// <summary>
        /// Get bid details — public, no auth required.
        /// </summary>
        [HttpGet("{bidId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBidDetails(int bidId)
        {
            var bid =
                await _biddingService
                    .GetBidDetailAsync(bidId);

            if (bid == null)
            {
                return NotFound(new
                {
                    message = "Bid not found."
                });
            }

            return Ok(bid);
        }

        #endregion

        #region DELETE BID

        /// <summary>
        /// Cancel bid
        /// </summary>
        [HttpDelete("{bidId}")]
        public async Task<IActionResult> DeleteBid(
            int bidId)
        {
            if (!int.TryParse(User.FindFirst("companyId")?.Value, out int companyId) || companyId <= 0)
                return Unauthorized(new { success = false, message = "Only verified company users can cancel bids." });

            var success =
                await _biddingService
                    .DeleteBidAsync(bidId, companyId);

            if (!success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Unable to cancel bid."
                });
            }

            return NoContent();
        }

        #endregion
    }
}
