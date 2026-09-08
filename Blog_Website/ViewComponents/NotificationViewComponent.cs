using Blog_Website.Models.Entities;
using Blog_Website.Services.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Blog_Website.ViewComponents
{
    //public class NotificationViewComponent(INotificationService _notificationService, UserManager<ApplicationUser> _userManager) : ViewComponent
    //{

    //    public async Task<IViewComponentResult> InvokeAsync()
    //    {
    //        var user = await _userManager.GetUserAsync(HttpContext.User);
    //        if (user == null) 
    //            return View(new List<Notification>());

    //        var notifications = await _notificationService.GetUserNotificationsAsync(user!.Id);

    //        return View(notifications);
    //    }
    //}

    public class NotificationViewComponent(INotificationService _notificationService, UserManager<ApplicationUser> _userManager) : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(int page = 1)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null)
                return View(new List<Notification>());

            var notifications = await _notificationService.GetUserNotificationsAsync(user.Id, page, 5);

            return View(notifications);
        }
    }

}
