using Blog_Website.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;


namespace Blog_Website.Controllers
{
    [Authorize]
    public class FollowController : Controller
    {
        private readonly IFollowService _followService;

        public FollowController(IFollowService followService)
        {
            _followService = followService;
        }

        [HttpPost]
        public async Task<IActionResult> AddFollow(string targetUserId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _followService.FollowAsync(targetUserId, currentUserId);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFollow(string targetUserId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _followService.UnfollowAsync(targetUserId, currentUserId);

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetFollowers(string userId)
        {
            var followers = await _followService.GetFollowersAsync(userId);

            return View(followers);
        }

        [HttpGet]
        public async Task<IActionResult> GetFollowing(string userId)
        {
            var followings = await _followService.GetFollowingsAsync(userId);

            return View(followings);
        }
    }
}
