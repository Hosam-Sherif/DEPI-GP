using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mazaad.Domain.Models
{
    public class Messages
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Channel")]
        public int ChannelId { get; set; }

        [ForeignKey("SenderUser")]
        public int SenderUserId { get; set; }

        public string MessageText { get; set; }
        public DateTime SentAt { get; set; }

        public Chat_Channels Channel { get; set; }
        public App_Users SenderUser { get; set; }
    }
}
