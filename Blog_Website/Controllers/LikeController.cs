using Blog_Website.Enums;
using Blog_Website.Models.Entities;
using Blog_Website.Services.IServices;
using Blog_Website.ViewModel.Like;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;

namespace Blog_Website.Controllers
{
    public class LikeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILikeService _likeService;

        public LikeController(UserManager<ApplicationUser> userManager, ILikeService likeService)
        {
            _userManager = userManager;
            _likeService = likeService;
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(LikeViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            var userId = user?.Id;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var hasLiked = await _likeService.IsLikedAsync(model.TargetId, userId, model.TargetType);

            if (hasLiked)
            {
                await _likeService.UnlikeAsync(model.TargetId, userId, model.TargetType);
            }
            else
                await _likeService.LikeAsync(model.TargetId, userId, model.TargetType);

            var likeCount = await _likeService.LikesCountAsync(model.TargetId, model.TargetType);

            return Json(new 
            {
                success = true,
                liked = !hasLiked,
                count = likeCount
            });
        }

        // Modal عشان ال 
        [HttpGet]
        public async Task<IActionResult> GetPostLikes(int postId)
        {
            var likes = await _likeService.GetAllLikesAsync(postId, LikeTargetType.Post);

            // نرجع اسم المستخدم وصورته فقط مثلاً
            var users = likes.Select(u => new
            {
                u.UserName,
                u.ImageURL,
                u.Id
            });

            return Json(users);
        }

    }
}
