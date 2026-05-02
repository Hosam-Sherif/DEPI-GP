using System.Collections.Generic;
using System.Threading.Tasks;
using Mazaad.Application.DTOs;

namespace Mazaad.Application.Interfaces
{
    public interface IChatService
    {
        Task<int> CreateOrGetChannelAsync(int listingId, int buyerCompanyId, int sellerCompanyId);
        Task<IEnumerable<MessageResponseDto>> GetChannelHistoryAsync(int channelId);

        // For SignalR: Save the message to DB before broadcasting it
        Task<MessageResponseDto> SaveMessageAsync(int channelId, int senderUserId, string text);
    }
}