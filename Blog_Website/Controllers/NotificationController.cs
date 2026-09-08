using Blog_Website.Models.Entities;
using Blog_Website.Services.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Blog_Website.Controllers
{
    public class NotificationController(INotificationService _notificationService, UserManager<ApplicationUser> _userManager) : Controller
    {
        public IActionResult Refresh()
        {
            return ViewComponent("Notification");
        }

        public async Task<IActionResult> Read(int id)
        {
            var notification = await _notificationService.MarkAsReadAsync(id);

            if (!string.IsNullOrEmpty(notification?.RedirectUrl))
            {
                return Redirect(notification.RedirectUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Load(int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var notifications = await _notificationService.GetUserNotificationsAsync(user.Id, page, 5);
            return ViewComponent("Notification", new { page });
        }
    }
}
