using Blog_Website.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace Blog_Website.Controllers
{
    public class NotificationController(INotificationService _notificationService) : Controller
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
    }
}
