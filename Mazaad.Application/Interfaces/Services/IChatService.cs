using System.Collections.Generic;
using System.Threading.Tasks;
using Mazaad.Application.DTOs;

namespace Mazaad.Application.Interfaces.Services
{
    public interface IChatService
    {
        /// <summary>Create or get an existing channel for a buyer+seller pair on a listing.</summary>
        Task<int> CreateOrGetChannelAsync(int listingId, int buyerCompanyId, int sellerCompanyId);

        /// <summary>Get all channels where a company is either buyer or seller.</summary>
        Task<IEnumerable<ChatChannelDto>> GetMyChannelsAsync(int companyId);

        /// <summary>Get full detail of a single channel (includes listing + company names).</summary>
        Task<ChatChannelDto?> GetChannelDetailAsync(int channelId);

        /// <summary>Get all messages in a channel ordered by time.</summary>
        Task<IEnumerable<MessageResponseDto>> GetChannelHistoryAsync(int channelId);

        /// <summary>Save and return a new message — used by both REST and SignalR.</summary>
        Task<MessageResponseDto> SaveMessageAsync(int channelId, int senderUserId, string text);

        /// <summary>Close / archive a channel (sets status = Closed).</summary>
        Task<bool> CloseChannelAsync(int channelId, int companyId);
    }
}