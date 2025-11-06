using Blog_Website.Models.Entities;
using Blog_Website.ViewModel.Notification;

namespace Blog_Website.Services.IServices
{
    public interface INotificationService
    {
        Task CreateAsync(AddNotificationViewModel notificationModel);

        Task<List<Notification>> GetUserNotificationsAsync(string userId);

        Task SendNotificationAsync(Notification notification);

        Task<Notification> MarkAsReadAsync(int id);
    }
}
