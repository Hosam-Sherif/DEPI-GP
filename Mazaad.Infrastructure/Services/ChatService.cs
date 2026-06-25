using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mazaad.Application.DTOs;
using Mazaad.Application.Interfaces.Services;
using Mazaad.Domain.Models;
using Mazaad.Domain.Enums;
using Mazaad.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mazaad.Infrastructure.Services
{
    public class ChatService : IChatService
    {
        private readonly AppDbContext _context;

        public ChatService(AppDbContext context)
        {
            _context = context;
        }

        // ── Create or Get Channel ─────────────────────────────────────────────────

        public async Task<int> CreateOrGetChannelAsync(int listingId, int buyerCompanyId, int sellerCompanyId)
        {
            var existing = await _context.ChatChannels
                .FirstOrDefaultAsync(c =>
                    c.ListingId == listingId &&
                    c.BuyerCompanyId == buyerCompanyId);

            if (existing != null)
                return existing.Id;

            var channel = new Chat_Channels
            {
                ListingId = listingId,
                BuyerCompanyId = buyerCompanyId,
                SellerCompanyId = sellerCompanyId,
                Status = ChannelStatus.Open,
                CreatedAt = DateTime.UtcNow
            };

            _context.ChatChannels.Add(channel);
            await _context.SaveChangesAsync();

            return channel.Id;
        }

        // ── List My Channels ──────────────────────────────────────────────────────

        public async Task<IEnumerable<ChatChannelDto>> GetMyChannelsAsync(int companyId)
        {
            var channels = await _context.ChatChannels
                .Include(c => c.Listing)
                .Include(c => c.BuyerCompany)
                .Include(c => c.SellerCompany)
                .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
                .Where(c => c.BuyerCompanyId == companyId || c.SellerCompanyId == companyId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return channels.Select(c => MapToChannelDto(c));
        }

        // ── Get Single Channel Detail ─────────────────────────────────────────────

        public async Task<ChatChannelDto?> GetChannelDetailAsync(int channelId)
        {
            var channel = await _context.ChatChannels
                .Include(c => c.Listing)
                .Include(c => c.BuyerCompany)
                .Include(c => c.SellerCompany)
                .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
                .FirstOrDefaultAsync(c => c.Id == channelId);

            return channel == null ? null : MapToChannelDto(channel);
        }

        // ── Channel History ───────────────────────────────────────────────────────

        public async Task<IEnumerable<MessageResponseDto>> GetChannelHistoryAsync(int channelId)
        {
            var messages = await _context.Messages
                .Where(m => m.ChannelId == channelId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            return messages.Select(MapToMessageDto);
        }

        // ── Send Message (REST + SignalR) ─────────────────────────────────────────

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

            return MapToMessageDto(message);
        }

        // ── Close Channel ─────────────────────────────────────────────────────────

        public async Task<bool> CloseChannelAsync(int channelId, int companyId)
        {
            var channel = await _context.ChatChannels.FindAsync(channelId);

            if (channel == null) return false;

            // Only buyer or seller can close the channel
            if (channel.BuyerCompanyId != companyId && channel.SellerCompanyId != companyId)
                return false;

            channel.Status = ChannelStatus.Closed;
            await _context.SaveChangesAsync();
            return true;
        }

        // ── Private Helpers ───────────────────────────────────────────────────────

        private static ChatChannelDto MapToChannelDto(Chat_Channels c) => new ChatChannelDto
        {
            Id = c.Id,
            ListingId = c.ListingId,
            ListingTitle = c.Listing?.Title ?? string.Empty,
            BuyerCompanyId = c.BuyerCompanyId,
            BuyerCompanyName = c.BuyerCompany?.CompanyName ?? string.Empty,
            SellerCompanyId = c.SellerCompanyId,
            SellerCompanyName = c.SellerCompany?.CompanyName ?? string.Empty,
            Status = c.Status,
            CreatedAt = c.CreatedAt,
            LastMessage = c.Messages.Any()
                ? MapToMessageDto(c.Messages.OrderByDescending(m => m.SentAt).First())
                : null
        };

        private static MessageResponseDto MapToMessageDto(Messages m) => new MessageResponseDto
        {
            Id = m.Id,
            ChannelId = m.ChannelId,
            SenderUserId = m.SenderUserId,
            MessageText = m.MessageText,
            SentAt = m.SentAt
        };
    }
}
