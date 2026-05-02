using System.Collections.Generic;
using System.Threading.Tasks;
using Mazaad.Application.DTOs;

namespace Mazaad.Application.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationResponseDto>> GetUserNotificationsAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
        Task<bool> MarkAsReadAsync(int notificationId, int userId);
        Task<bool> MarkAllAsReadAsync(int userId);

        /// <summary>Create and persist a notification (called internally by BiddingService, OrderService etc.)</summary>
        Task CreateNotificationAsync(int userId, string title, string message, string referenceType, int referenceId);
    }
}
