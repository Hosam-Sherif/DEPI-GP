using System;
using Mazaad.Domain.Enums;

namespace Mazaad.Application.DTOs
{
    /// <summary>Summary of a chat channel returned in list/detail endpoints.</summary>
    public class ChatChannelDto
    {
        public int Id { get; set; }
        public int ListingId { get; set; }
        public string ListingTitle { get; set; } = string.Empty;
        public int BuyerCompanyId { get; set; }
        public string BuyerCompanyName { get; set; } = string.Empty;
        public int SellerCompanyId { get; set; }
        public string SellerCompanyName { get; set; } = string.Empty;
        public ChannelStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UnreadCount { get; set; }
        public MessageResponseDto? LastMessage { get; set; }
    }
}
