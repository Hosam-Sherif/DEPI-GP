using System.Collections.Concurrent;
using Mazaad.Application.DTOs;
using Mazaad.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Mazaad.API.Hubs
{
    /// <summary>
    /// Enterprise-grade real-time auction hub
    /// Production-ready architecture for:
    /// - Live bidding
    /// - Presence tracking
    /// - Heartbeats
    /// - Anonymous bidding
    /// - Realtime updates
    /// - Scalable SignalR usage
    /// </summary>
    [Authorize]
    public class AuctionHub : Hub
    {
        private readonly IBiddingService _biddingService;
        private readonly IAuctionPresenceService _presenceService;
        private readonly ILogger<AuctionHub> _logger;

        public AuctionHub(
            IBiddingService biddingService,
            IAuctionPresenceService presenceService,
            ILogger<AuctionHub> logger)
        {
            _biddingService = biddingService;
            _presenceService = presenceService;
            _logger = logger;
        }

        #region CONNECTION EVENTS

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation(
                "Client connected: {ConnectionId}",
                Context.ConnectionId);

            await Clients.Caller.SendAsync("Connected", new
            {
                connectionId = Context.ConnectionId,
                timestamp = DateTime.UtcNow,
                message = "Connected successfully"
            });

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var disconnectedUser =
                await _presenceService.RemoveConnectionAsync(Context.ConnectionId);

            if (disconnectedUser is not null)
            {
                await Groups.RemoveFromGroupAsync(
                    Context.ConnectionId,
                    GetAuctionGroup(disconnectedUser.ListingId));

                await Clients
                    .Group(GetAuctionGroup(disconnectedUser.ListingId))
                    .SendAsync("BidderDisconnected", new
                    {
                        connectionId = Context.ConnectionId,
                        bidder = disconnectedUser.DisplayName,
                        timestamp = DateTime.UtcNow,
                        activeBidders =
                            await _presenceService.GetAuctionParticipantsAsync(
                                disconnectedUser.ListingId)
                    });

                _logger.LogInformation(
                    "Bidder disconnected from auction {ListingId}",
                    disconnectedUser.ListingId);
            }

            if (exception != null)
            {
                _logger.LogError(
                    exception,
                    "SignalR disconnection error");
            }

            await base.OnDisconnectedAsync(exception);
        }

        #endregion

        #region AUCTION PRESENCE

        /// <summary>
        /// Join auction room and subscribe to live updates
        /// </summary>
        public async Task JoinAuction(
            int listingId,
            string displayName,
            bool isAnonymous = false)
        {
            var finalDisplayName =
                isAnonymous ? "Anonymous Bidder" : displayName;

            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                GetAuctionGroup(listingId));

            var participant = new AuctionParticipantDto
            {
                ConnectionId = Context.ConnectionId,
                ListingId = listingId,
                DisplayName = finalDisplayName,
                IsAnonymous = isAnonymous,
                JoinedAt = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow
            };

            await _presenceService.AddConnectionAsync(participant);

            var liveBids =
                await _biddingService.GetLiveBidsAsync(listingId);

            var activeParticipants =
                await _presenceService.GetAuctionParticipantsAsync(listingId);

            // Send initial room state only to caller
            await Clients.Caller.SendAsync("AuctionJoined", new
            {
                listingId,
                liveBids,
                activeBidders = activeParticipants,
                joinedAt = DateTime.UtcNow
            });

            // Notify room
            await Clients
                .Group(GetAuctionGroup(listingId))
                .SendAsync("BidderJoined", new
                {
                    bidder = finalDisplayName,
                    timestamp = DateTime.UtcNow,
                    activeBidders = activeParticipants
                });

            _logger.LogInformation(
                "{Bidder} joined auction {ListingId}",
                finalDisplayName,
                listingId);
        }

        /// <summary>
        /// Leave auction room
        /// </summary>
        public async Task LeaveAuction(int listingId)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                GetAuctionGroup(listingId));

            var removed =
                await _presenceService.RemoveConnectionAsync(
                    Context.ConnectionId);

            var activeParticipants =
                await _presenceService.GetAuctionParticipantsAsync(listingId);

            await Clients
                .Group(GetAuctionGroup(listingId))
                .SendAsync("BidderLeft", new
                {
                    connectionId = Context.ConnectionId,
                    bidder = removed?.DisplayName,
                    timestamp = DateTime.UtcNow,
                    activeBidders = activeParticipants
                });

            _logger.LogInformation(
                "Client left auction {ListingId}",
                listingId);
        }

        /// <summary>
        /// Keep connection alive and update activity timestamp
        /// </summary>
        public async Task Heartbeat(int listingId)
        {
            await _presenceService.UpdateHeartbeatAsync(
                Context.ConnectionId);

            await Clients.Caller.SendAsync("HeartbeatAck", new
            {
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Get current active bidders
        /// </summary>
        public async Task GetActiveBidders(int listingId)
        {
            var bidders =
                await _presenceService
                    .GetAuctionParticipantsAsync(listingId);

            await Clients.Caller.SendAsync(
                "ActiveBidders",
                bidders);
        }

        #endregion

        #region BIDDING

        /// <summary>
        /// Place secure live bid
        /// </summary>
        public async Task PlaceBid(
            int userId,
            int companyId,
            PlaceBidDto request)
        {
            try
            {
                var result =
                    await _biddingService.PlaceBidAsync(
                        userId,
                        companyId,
                        request);

                if (!result.Success)
                {
                    await Clients.Caller.SendAsync(
                        "BidRejected",
                        new
                        {
                            message = result.Message
                        });

                    return;
                }

                var liveUpdate = new LiveBidUpdateDto
                {
                    ListingId = request.ListingId,
                    DisplayBidderName = result.DisplayBiddersName,
                    NewHighestBid = result.NewPrice,
                    Timestamp = DateTime.UtcNow
                };

                // Broadcast to all auction participants
                await Clients
                    .Group(GetAuctionGroup(request.ListingId))
                    .SendAsync("BidPlaced", liveUpdate);

                _logger.LogInformation(
                    "Bid placed on auction {ListingId} with amount {Amount}",
                    request.ListingId,
                    result.NewPrice);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error placing bid");

                await Clients.Caller.SendAsync(
                    "BidError",
                    new
                    {
                        message = "An unexpected error occurred while placing the bid."
                    });
            }
        }

        #endregion

        #region PRIVATE HELPERS

        private static string GetAuctionGroup(int listingId)
            => $"auction-{listingId}";

        #endregion
    }
}