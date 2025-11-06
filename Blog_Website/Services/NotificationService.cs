using Blog_Website.Hubs;
using Blog_Website.Models.Data;
using Blog_Website.Models.Entities;
using Blog_Website.Services.IServices;
using Blog_Website.ViewModel.Notification;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Blog_Website.Services
{
    public class NotificationService(AppDbContext _context, IHubContext<NotificationHub> _hubContext) : INotificationService
    {
        public async Task CreateAsync(AddNotificationViewModel notificationModel)
        {
            var notification = new Notification
            {
                SenderId = notificationModel.SenderId,
                ReceiverId = notificationModel.ReceiverId,
                RedirectUrl = notificationModel.RedirectUrl,
                Title = notificationModel.Title,
                Description = notificationModel.Description,
                CreatedDate = DateTime.UtcNow,
                IsRead = false,
                Type = notificationModel.Type,
            };

            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.User(notification.ReceiverId!)
                .SendAsync("ReceiveNotification", notification.Title, notification.RedirectUrl);

            //await SendNotificationAsync(notification);
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(string userId)
        {
            var notifications = await _context.Notifications
                .Include(X => X.Sender)
                .Where(x => x.ReceiverId == userId)
                .OrderByDescending(X => X.CreatedDate)
                .ToListAsync();

            return notifications;
        }

        public async Task<Notification> MarkAsReadAsync(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);

            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                _context.Notifications.Update(notification);
                await _context.SaveChangesAsync();
            }

            return notification!; // رجعه بعد التعديل
        }

        public async Task SendNotificationAsync(Notification notification)
        {
            notification.Sender = await _context.Users
                .Where(x => x.Id == notification.SenderId)
                .FirstOrDefaultAsync();

            await _hubContext.Clients.User(notification.ReceiverId!).SendAsync("ReceiveNotification", notification.Description, notification.RedirectUrl);
        }
    }
}
