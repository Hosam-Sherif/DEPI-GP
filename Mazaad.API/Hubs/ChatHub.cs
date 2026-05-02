using System.Threading.Tasks;
using Mazaad.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Mazaad.API.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        // 1. Frontend calls this to enter a specific chat room
        public async Task JoinChannel(string channelId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, channelId);
        }

        // 2. Frontend calls this to send a message
        public async Task SendMessage(int channelId, int senderUserId, string messageText)
        {
            // First, save it to SQL Server using our Infrastructure service
            var savedMessage = await _chatService.SaveMessageAsync(channelId, senderUserId, messageText);

            // Then, instantly push it ONLY to the people in that specific chat room
            await Clients.Group(channelId.ToString()).SendAsync("ReceiveMessage", savedMessage);
        }
    }
}