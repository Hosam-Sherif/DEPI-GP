using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mazaad.Application.DTOs;
using Mazaad.Application.Interfaces.Services;
using Mazaad.Domain.Models;
using Mazaad.Infrastructure.Hubs;
using Mazaad.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Mazaad.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<AuctionHub> _hubContext;

        public NotificationService(AppDbContext context, IHubContext<AuctionHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<IEnumerable<NotificationResponseDto>> GetUserNotificationsAsync(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return notifications.Select(MapToDto);
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null) return false;

            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(int userId)
        {
            var unread = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (!unread.Any()) return false;

            foreach (var n in unread)
                n.IsRead = true;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task CreateNotificationAsync(int userId, string title, string message, string referenceType, int referenceId)
        {
            var notification = new Notifications
            {
                UserId = userId,
                Title = title,
                Message = message,
                IsRead = false,
                ReferenceType = referenceType,
                ReferenceId = referenceId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task NotifySellerNewBidAsync(int sellerCompanyId, string listingTitle, string bidderName, decimal bidAmount)
        {
            var title = "New Bid Placed";
            var message = $"A new bid of {bidAmount:C} has been placed on your listing '{listingTitle}' by {bidderName}.";

            // Save to DB for all users belonging to the seller company
            var userIds = await _context.Users
                .Where(u => u.CompanyId == sellerCompanyId)
                .Select(u => u.Id)
                .ToListAsync();

            foreach (var userId in userIds)
            {
                var notification = new Notifications
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    IsRead = false,
                    ReferenceType = "Listing",
                    ReferenceId = 0,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Notifications.Add(notification);
            }
            await _context.SaveChangesAsync();

            // Send via SignalR strictly to the specific company's group
            await _hubContext.Clients.Group($"Company_{sellerCompanyId}").SendAsync("ReceiveNotification", message);
        }

        public async Task NotifyWinnerAsync(int winnerCompanyId, string listingTitle, decimal winningAmount)
        {
            var title = "Auction Won!";
            var message = $"Congratulations! Your company won the auction for '{listingTitle}' with a winning bid of {winningAmount:C}.";

            // Save to DB for all users belonging to the winner company
            var userIds = await _context.Users
                .Where(u => u.CompanyId == winnerCompanyId)
                .Select(u => u.Id)
                .ToListAsync();

            foreach (var userId in userIds)
            {
                var notification = new Notifications
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    IsRead = false,
                    ReferenceType = "Listing",
                    ReferenceId = 0,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Notifications.Add(notification);
            }
            await _context.SaveChangesAsync();

            // Send via SignalR strictly to the specific company's group
            await _hubContext.Clients.Group($"Company_{winnerCompanyId}").SendAsync("ReceiveNotification", message);
        }

        private static NotificationResponseDto MapToDto(Notifications n) => new NotificationResponseDto
        {
            Id = n.Id,
            UserId = n.UserId,
            Title = n.Title,
            Message = n.Message,
            IsRead = n.IsRead,
            ReferenceType = n.ReferenceType,
            ReferenceId = n.ReferenceId,
            CreatedAt = n.CreatedAt
        };
    }
}
