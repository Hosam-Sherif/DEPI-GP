using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mazaad.Application.Interfaces;

namespace Mazaad.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartChat(int listingId, int buyerCompanyId, int sellerCompanyId)
        {
            var channelId = await _chatService.CreateOrGetChannelAsync(listingId, buyerCompanyId, sellerCompanyId);
            return Ok(new { ChannelId = channelId });
        }

        [HttpGet("{channelId}/history")]
        public async Task<IActionResult> GetHistory(int channelId)
        {
            var history = await _chatService.GetChannelHistoryAsync(channelId);
            return Ok(history);
        }
    }
}