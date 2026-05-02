using System.Threading.Tasks;
using Mazaad.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Mazaad.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>Get all notifications for the current user.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // TODO: extract from JWT
            int currentUserId = 1;
            var notifications = await _notificationService.GetUserNotificationsAsync(currentUserId);
            return Ok(notifications);
        }

        /// <summary>Get unread notification count (for the notification bell badge).</summary>
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            int currentUserId = 1;
            var count = await _notificationService.GetUnreadCountAsync(currentUserId);
            return Ok(new { UnreadCount = count });
        }

        /// <summary>Mark a single notification as read.</summary>
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            int currentUserId = 1;
            var success = await _notificationService.MarkAsReadAsync(id, currentUserId);
            if (!success) return NotFound();
            return NoContent();
        }

        /// <summary>Mark all notifications as read.</summary>
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            int currentUserId = 1;
            await _notificationService.MarkAllAsReadAsync(currentUserId);
            return NoContent();
        }
    }
}
