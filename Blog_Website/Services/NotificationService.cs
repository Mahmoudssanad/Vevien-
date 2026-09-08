using Blog_Website.Hubs;
using Blog_Website.Models.Data;
using Blog_Website.Models.Entities;
using Blog_Website.Services.IServices;
using Blog_Website.ViewModel.Notification;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Blog_Website.Services
{
    public class NotificationService(DbContextOptions<AppDbContext> _context, IHubContext<NotificationHub> _hubContext) : INotificationService
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

            using var ctx = new AppDbContext(_context);
            await ctx.Notifications.AddAsync(notification);
            await ctx.SaveChangesAsync();


            await _hubContext.Clients.User(notification.ReceiverId!)
                .SendAsync("ReceiveNotification", notification.Title, notification.RedirectUrl);
        }

        //public async Task<List<Notification>> GetUserNotificationsAsync(string userId)
        //{
        //    var notifications = await _context.Notifications
        //        .Include(X => X.Sender)
        //        .Where(x => x.ReceiverId == userId)
        //        .OrderByDescending(X => X.CreatedDate)
        //        .ToListAsync();

        //    return notifications;
        //}

        public async Task<List<Notification>> GetUserNotificationsAsync(string userId, int pageNumber = 1, int pageSize = 5)
        {
            using var ctx = new AppDbContext(_context);

            var query = ctx.Notifications
                .Include(x => x.Sender)
                .AsNoTracking()
                .Where(x => x.ReceiverId == userId && !x.Receiver!.IsDeleted)
                .OrderByDescending(x => x.CreatedDate);

            var notifications = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return notifications;
        }

        public async Task<Notification> MarkAsReadAsync(int id)
        {
            using var ctx = new AppDbContext(_context);

            var notification = await ctx.Notifications.FindAsync(id);

            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                ctx.Notifications.Update(notification);
                await ctx.SaveChangesAsync();
            }

            return notification!; // رجعه بعد التعديل
        }

        public async Task SendNotificationAsync(Notification notification)
        {
            using var ctx = new AppDbContext(_context);

            notification.Sender = await ctx.Users
                .Where(x => x.Id == notification.SenderId && !x.IsDeleted)
                .FirstOrDefaultAsync();

            await _hubContext.Clients.User(notification.ReceiverId!).SendAsync("ReceiveNotification", notification.Description, notification.RedirectUrl);
        }
    }
}
