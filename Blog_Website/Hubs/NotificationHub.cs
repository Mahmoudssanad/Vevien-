using Microsoft.AspNetCore.SignalR;

namespace Blog_Website.Hubs
{
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            // لو بتستخدم Authentication لازم SignalR تعرف المستخدم
            await base.OnConnectedAsync();
        }
    }
}
