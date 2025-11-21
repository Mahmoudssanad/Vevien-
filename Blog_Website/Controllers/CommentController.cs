using Blog_Website.Hubs;
using Blog_Website.Services.IServices;
using Blog_Website.ViewModel.Comments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Blog_Website.Controllers
{
    public class CommentController : Controller
    {
        private readonly ICommentService _commentService;
        private readonly IHubContext<CommentHub> _hubContext;
        private readonly IViewRenderService _viewRenderService;

        public CommentController(ICommentService commentService, IHubContext<CommentHub> hubContext, IViewRenderService viewRenderService)
        {
            _commentService = commentService;
            _hubContext = hubContext;
            _viewRenderService = viewRenderService;
        }

        public async Task<IActionResult> AddComment(CommentViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            model.UserId = userId;

            var comment = await _commentService.AddCommentAsync(model);

            #region SignalR
            //var commentHtml = await _viewRenderService.RenderToStringAsync("_CommentPartial", comment);
            //await _hubContext.Clients.All.SendAsync("ReceiveComment", model.PostId, commentHtml);
            #endregion

            //return Ok();

            return PartialView("_CommentPartial", comment);
        }


    }
}
