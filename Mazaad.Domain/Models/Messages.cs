using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mazaad.Domain.Models
{
    public class Messages
    {
        public int Id { get; set; }

        [ForeignKey("Channel")]
        public int channel_id { get; set; }

        [ForeignKey("SenderUser")]
        public int sender_user_id { get; set; }

        public string message_text { get; set; }
        public DateTime sent_at { get; set; }

        public Chat_Channels Channel { get; set; }
        public App_Users SenderUser { get; set; }
    }
}
