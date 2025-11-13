using Blog_Website.Models.Entities;
using Blog_Website.ViewModel.Notification;

namespace Blog_Website.Services.IServices
{
    public interface INotificationService
    {
        Task CreateAsync(AddNotificationViewModel notificationModel);

        //Task<List<Notification>> GetUserNotificationsAsync(string userId);

        Task<List<Notification>> GetUserNotificationsAsync(string userId, int pageNumber = 1, int pageSize = 5);

        Task SendNotificationAsync(Notification notification);

        Task<Notification> MarkAsReadAsync(int id);
    }
}
