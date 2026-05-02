using System;

namespace Mazaad.Application.DTOs
{
    public class MessageResponseDto
    {
        public int Id { get; set; }
        public int ChannelId { get; set; }
        public int SenderUserId { get; set; }
        public string MessageText { get; set; }
        public DateTime SentAt { get; set; }
    }
}