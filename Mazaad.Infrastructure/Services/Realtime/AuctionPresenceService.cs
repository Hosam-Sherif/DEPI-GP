using Mazaad.Application.DTOs;
using Mazaad.Application.Interfaces.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mazaad.Infrastructure.Services
{
    public class AuctionPresenceService : IAuctionPresenceService
    {
        private static readonly ConcurrentDictionary<string, AuctionParticipantDto>
            Connections = new();

        public Task AddConnectionAsync(AuctionParticipantDto participant)
        {
            Connections[participant.ConnectionId] = participant;
            return Task.CompletedTask;
        }

        public Task<AuctionParticipantDto?> RemoveConnectionAsync(
            string connectionId)
        {
            Connections.TryRemove(connectionId, out var removed);

            return Task.FromResult(removed);
        }

        public Task UpdateHeartbeatAsync(string connectionId)
        {
            if (Connections.TryGetValue(connectionId, out var conn))
            {
                conn.LastActivity = DateTime.UtcNow;
            }

            return Task.CompletedTask;
        }

        public Task<List<AuctionParticipantDto>>
            GetAuctionParticipantsAsync(int listingId)
        {
            var result = Connections.Values
                .Where(x => x.ListingId == listingId)
                .OrderByDescending(x => x.LastActivity)
                .ToList();

            return Task.FromResult(result);
        }
    }
}