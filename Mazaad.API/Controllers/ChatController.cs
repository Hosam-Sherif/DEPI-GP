using System.Security.Claims;
using Mazaad.Application.DTOs;
using Mazaad.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Mazaad.API.Hubs;

namespace Mazaad.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IHubContext<ChatHub> _chatHubContext;

        public ChatController(IChatService chatService, IHubContext<ChatHub> chatHubContext)
        {
            _chatService = chatService;
            _chatHubContext = chatHubContext;
        }

        // ── Channels ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Start or retrieve an existing chat channel between buyer and seller for a listing.
        /// Only members of the buyer company can initiate a chat.
        /// </summary>
        [HttpPost("start")]
        public async Task<IActionResult> StartChat(
            [FromQuery] int listingId,
            [FromQuery] int sellerCompanyId)
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Unauthorized(new { error = "A valid company account is required." });

            // الشركة الحالية هي الـ buyer تلقائياً
            if (companyId.Value == sellerCompanyId)
                return BadRequest(new { error = "Cannot start a chat with your own listing." });

            var channelId = await _chatService.CreateOrGetChannelAsync(
                listingId, companyId.Value, sellerCompanyId);

            return Ok(new { channelId });
        }

        /// <summary>
        /// Get all chat channels for the current user's company.
        /// </summary>
        [HttpGet("my-channels")]
        public async Task<IActionResult> GetMyChannels()
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Unauthorized(new { error = "A valid company account is required." });

            var channels = await _chatService.GetMyChannelsAsync(companyId.Value);
            return Ok(channels);
        }

        /// <summary>
        /// Get detail for a single chat channel.
        /// Only buyer or seller of the channel can view it.
        /// </summary>
        [HttpGet("{channelId}")]
        public async Task<IActionResult> GetChannelDetail(int channelId)
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Unauthorized(new { error = "A valid company account is required." });

            var channel = await _chatService.GetChannelDetailAsync(channelId);
            if (channel == null)
                return NotFound(new { error = "Channel not found." });

            // ownership check
            if (channel.BuyerCompanyId != companyId.Value && channel.SellerCompanyId != companyId.Value)
                return Forbid();

            return Ok(channel);
        }

        /// <summary>
        /// Get all messages in a channel ordered by time.
        /// Only buyer or seller of the channel can view history.
        /// </summary>
        [HttpGet("{channelId}/history")]
        public async Task<IActionResult> GetHistory(int channelId)
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Unauthorized(new { error = "A valid company account is required." });

            // ownership check عن طريق channel detail
            var channel = await _chatService.GetChannelDetailAsync(channelId);
            if (channel == null)
                return NotFound(new { error = "Channel not found." });

            if (channel.BuyerCompanyId != companyId.Value && channel.SellerCompanyId != companyId.Value)
                return Forbid();

            var messages = await _chatService.GetChannelHistoryAsync(channelId);
            return Ok(messages);
        }

        /// <summary>
        /// Send a message via REST + broadcast via SignalR.
        /// Only buyer or seller of the channel can send messages.
        /// </summary>
        [HttpPost("{channelId}/messages")]
        public async Task<IActionResult> SendMessage(int channelId, [FromBody] SendMessageDto request)
        {
            var userId = GetCurrentUserId();
            var companyId = GetCurrentCompanyId();

            if (userId == null || companyId == null)
                return Unauthorized(new { error = "Invalid user token." });

            if (string.IsNullOrWhiteSpace(request.MessageText))
                return BadRequest(new { error = "Message text cannot be empty." });

            // ownership check
            var channel = await _chatService.GetChannelDetailAsync(channelId);
            if (channel == null)
                return NotFound(new { error = "Channel not found." });

            if (channel.BuyerCompanyId != companyId.Value && channel.SellerCompanyId != companyId.Value)
                return Forbid();

            var saved = await _chatService.SaveMessageAsync(channelId, userId.Value, request.MessageText);

            await _chatHubContext.Clients
                .Group(channelId.ToString())
                .SendAsync("ReceiveMessage", saved);

            return Ok(saved);
        }

        /// <summary>
        /// Close a chat channel. Only buyer or seller can close it.
        /// </summary>
        [HttpDelete("{channelId}")]
        public async Task<IActionResult> CloseChannel(int channelId)
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Unauthorized(new { error = "A valid company account is required." });

            var success = await _chatService.CloseChannelAsync(channelId, companyId.Value);
            if (!success)
                return NotFound(new { error = "Channel not found or you are not a participant." });

            return NoContent();
        }

        // ── Private Helpers ───────────────────────────────────────────────────────

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst("uid")?.Value
                     ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) && id > 0 ? id : null;
        }

        private int? GetCurrentCompanyId()
        {
            var claim = User.FindFirst("companyId")?.Value;
            return int.TryParse(claim, out var id) && id > 0 ? id : null;
        }
    }
}