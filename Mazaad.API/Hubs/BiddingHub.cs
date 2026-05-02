using System.Threading.Tasks;
using Mazaad.Application.DTOs;
using Mazaad.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Mazaad.API.Hubs
{
    /// <summary>
    /// Real-time bidding hub. Clients join an auction group via JoinAuction(listingId)
    /// and receive BidPlaced events whenever a new bid is placed through the REST API
    /// or directly via PlaceBid from the hub.
    /// </summary>
    public class BiddingHub : Hub
    {
        private readonly IBiddingService _biddingService;

        public BiddingHub(IBiddingService biddingService)
        {
            _biddingService = biddingService;
        }

        /// <summary>
        /// Client calls this on entering the bidding room to subscribe to live updates.
        /// Group name pattern: "listing-{listingId}"
        /// </summary>
        public async Task JoinAuction(int listingId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"listing-{listingId}");

            // Send the current live bids to the newly joined client only
            var liveBids = await _biddingService.GetLiveBidsAsync(listingId);
            await Clients.Caller.SendAsync("InitialBidState", liveBids);
        }

        /// <summary>Client calls this when navigating away from the bidding room.</summary>
        public async Task LeaveAuction(int listingId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"listing-{listingId}");
        }

        /// <summary>
        /// Client can also place a bid directly via SignalR (used by the "Place Secure Bid" button).
        /// Broadcasts BidPlaced to all group members on success.
        /// </summary>
        public async Task PlaceBid(int userId, int companyId, PlaceBidDto request)
        {
            var result = await _biddingService.PlaceBidAsync(userId, companyId, request);

            if (!result.Success)
            {
                // Only the caller gets the failure message
                await Clients.Caller.SendAsync("BidFailed", result.Message);
                return;
            }

            var liveUpdate = new LiveBidUpdateDto
            {
                ListingId = request.ListingId,
                DisplayBidderName = result.DisplayBiddersName,
                NewHighestBid = result.NewPrice,
                Timestamp = System.DateTime.UtcNow
            };

            // Broadcast to everyone in the auction room, including the bidder
            await Clients.Group($"listing-{request.ListingId}").SendAsync("BidPlaced", liveUpdate);
        }
    }
}
