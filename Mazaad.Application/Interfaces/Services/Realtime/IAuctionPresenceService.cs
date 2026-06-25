using Mazaad.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mazaad.Application.Interfaces.Services
{
    public interface IAuctionPresenceService
    {
        Task AddConnectionAsync(AuctionParticipantDto participant);

        Task<AuctionParticipantDto?> RemoveConnectionAsync(
            string connectionId);

        Task UpdateHeartbeatAsync(string connectionId);

        Task<List<AuctionParticipantDto>>
            GetAuctionParticipantsAsync(int listingId);
    }
}
