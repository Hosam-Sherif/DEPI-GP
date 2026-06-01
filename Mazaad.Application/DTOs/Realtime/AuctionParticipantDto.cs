using System;

namespace Mazaad.Application.DTOs
{
    public class AuctionParticipantDto
    {
        public string ConnectionId { get; set; } = default!;

        public int ListingId { get; set; }

        public string DisplayName { get; set; } = default!;

        public bool IsAnonymous { get; set; }

        public DateTime JoinedAt { get; set; }

        public DateTime LastActivity { get; set; }
    }
}