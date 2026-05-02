using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mazaad.Application.DTOs;
using Mazaad.Application.Interfaces;
using Mazaad.Domain.Models;
using Mazaad.Domain.Enums;
using Mazaad.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mazaad.Application.Services
{
    public class ChatService : IChatService
    {
        private readonly AppDbContext _context;

        public ChatService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> CreateOrGetChannelAsync(int listingId, int buyerCompanyId, int sellerCompanyId)
        {
            var existingChannel = await _context.ChatChannels
                .FirstOrDefaultAsync(c => c.ListingId == listingId && c.BuyerCompanyId == buyerCompanyId);

            if (existingChannel != null)
                return existingChannel.Id;

            var newChannel = new Chat_Channels
            {
                ListingId = listingId,
                BuyerCompanyId = buyerCompanyId,
                SellerCompanyId = sellerCompanyId,
                Status = ChannelStatus.Open,
                CreatedAt = DateTime.UtcNow
            };

            _context.ChatChannels.Add(newChannel);
            await _context.SaveChangesAsync();

            return newChannel.Id;
        }

        public async Task<IEnumerable<MessageResponseDto>> GetChannelHistoryAsync(int channelId)
        {
            var messages = await _context.Messages
                .Where(m => m.ChannelId == channelId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            return messages.Select(m => new MessageResponseDto
            {
                Id = m.Id,
                ChannelId = m.ChannelId,
                SenderUserId = m.SenderUserId,
                MessageText = m.MessageText,
                SentAt = m.SentAt
            });
        }

        public async Task<MessageResponseDto> SaveMessageAsync(int channelId, int senderUserId, string text)
        {
            var message = new Messages
            {
                ChannelId = channelId,
                SenderUserId = senderUserId,
                MessageText = text,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return new MessageResponseDto
            {
                Id = message.Id,
                ChannelId = message.ChannelId,
                SenderUserId = message.SenderUserId,
                MessageText = message.MessageText,
                SentAt = message.SentAt
            };
        }
    }
}
