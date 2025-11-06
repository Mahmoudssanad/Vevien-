using Blog_Website.Extentions;
using Blog_Website.Hubs;
using Blog_Website.Services.IServices;
using Blog_Website.ViewModel.Comment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Blog_Website.Controllers
{
    public class CommentController : Controller
    {
        private readonly ICommentService _commentService;
        private readonly IHubContext<CommentHub> _hubContext;

        public CommentController(ICommentService commentService, IHubContext<CommentHub> hubContext)
        {
            _commentService = commentService;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> AddComment(CommentViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            model.UserId = userId;

            var comment = await _commentService.AddCommentAsync(model);

            var partialView = await this.RenderViewAsync("_CommentPartial", comment, true);

            // 🧠 نبعت الإشعار عبر SignalR لكل المستخدمين (أو حسب الحاجة)
            await _hubContext.Clients.All.SendAsync("ReceiveComment", model.PostId, partialView);

            return Ok();

            //return PartialView("_CommentPartial", comment);
        }
    }
}
