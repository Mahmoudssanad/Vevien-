using Blog_Website.Hubs;
using Blog_Website.Services.IServices;
using Blog_Website.ViewModel.Comments;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Blog_Website.Controllers
{
    public class CommentController : Controller
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(CommentViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            model.UserId = userId;

            var comment = await _commentService.AddCommentAsync(model);

            return PartialView("_CommentPartial", comment);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int commentId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId is null)
                return Unauthorized();

            var result = await _commentService.DeleteCommentAsync(commentId, currentUserId);

            return Json(result);
        }

    }
}
