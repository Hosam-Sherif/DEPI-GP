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
        /// Returns { channelId } on success.
        /// Note: buyerCompanyId and sellerCompanyId must be valid company IDs in the database.
        /// </summary>
        [HttpPost("start")]
        public async Task<IActionResult> StartChat(
            [FromQuery] int listingId,
            [FromQuery] int buyerCompanyId,
            [FromQuery] int sellerCompanyId)
        {
            var channelId = await _chatService.CreateOrGetChannelAsync(
                listingId, buyerCompanyId, sellerCompanyId);

            return Ok(new { channelId });
        }

        /// <summary>
        /// Get all chat channels for the current user's company.
        /// Returns a summary list with last message preview and channel metadata.
        /// </summary>
        [HttpGet("my-channels")]
        [Authorize]
        public async Task<IActionResult> GetMyChannels()
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Unauthorized(new { error = "A valid company account is required." });

            var channels = await _chatService.GetMyChannelsAsync(companyId.Value);
            return Ok(channels);
        }

        /// <summary>
        /// Get detail for a single chat channel (includes listing title, company names, status).
        /// </summary>
        [HttpGet("{channelId}")]
        public async Task<IActionResult> GetChannelDetail(int channelId)
        {
            var channel = await _chatService.GetChannelDetailAsync(channelId);
            if (channel == null) return NotFound(new { error = "Channel not found." });
            return Ok(channel);
        }

        /// <summary>
        /// Get all messages in a channel ordered by time (oldest first).
        /// </summary>
        [HttpGet("{channelId}/history")]
        public async Task<IActionResult> GetHistory(int channelId)
        {
            var messages = await _chatService.GetChannelHistoryAsync(channelId);
            return Ok(messages);
        }

        /// <summary>
        /// Send a message via REST. Saves the message to the database and then
        /// broadcasts it via SignalR to all connected clients in the channel group.
        /// Requires authentication. The senderUserId is extracted from the JWT token.
        /// </summary>
        [HttpPost("{channelId}/messages")]
        [Authorize]
        public async Task<IActionResult> SendMessage(int channelId, [FromBody] SendMessageDto request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { error = "Invalid user token." });

            if (string.IsNullOrWhiteSpace(request.MessageText))
                return BadRequest(new { error = "Message text cannot be empty." });

            // Save to DB
            var saved = await _chatService.SaveMessageAsync(channelId, userId.Value, request.MessageText);

            // Broadcast to all SignalR clients in this channel's group
            await _chatHubContext.Clients
                .Group(channelId.ToString())
                .SendAsync("ReceiveMessage", saved);

            return Ok(saved);
        }

        /// <summary>
        /// Close a chat channel. Only members of the buyer or seller company can close it.
        /// </summary>
        [HttpDelete("{channelId}")]
        [Authorize]
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