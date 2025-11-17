using Blog_Website.Models.Entities;
using Blog_Website.Services.IServices;
using Blog_Website.ViewModel.Profile;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Blog_Website.Services
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IFollowService _followService;
        private readonly IPostService _postService;

        public ProfileService
            (
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContext,
            IFollowService followService,
            IPostService postService)
        {
            _userManager = userManager;
            _httpContext = httpContext;
            _followService = followService;
            _postService = postService;
        }

        public async Task<bool> SoftDeleteUserAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException("User is not logged in.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            // Soft Delete
            user.IsDeleted = true;

            // Modify email to avoid unique constraint => علشان نقدر نعمل استرجاع للحساب المحذف لازم نشيل الخطوه دي
            //user.Email = $"{user.Email}__deleted__{Guid.NewGuid()}";

            // UpdateSecurityStampAsync => علي الانتهاء علشان يعيد تسجيل الدخول تاني cookie بيجبر ال 
            await _userManager.UpdateSecurityStampAsync(user);

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to delete user: {errors}");
            }

            return true;
        }

        public async Task<List<ProfileViewModel>> GetAllAsync()
        {
            var currentUserId = _httpContext.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var followings = await _followService.GetFollowingsAsync(currentUserId!);

            var followingsUserId = followings.Select(x => x.Id);

            var allUsers = await _userManager.Users
                .Where(x => x.Id != currentUserId && !followingsUserId.Contains(x.Id) && !x.IsDeleted)
                .ToListAsync();

            return allUsers.Select(u => new ProfileViewModel
            {
                Email = u.Email!,
                UserName = u.UserName!,
                UserId = u.Id,
                Image = u.ImageURL!,
            }).ToList();
        }

        public async Task<IdentityResult> ChangePassword(ChangePasswordViewModel model)
        {
            var userId = _httpContext.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _userManager.FindByIdAsync(userId!);

            if(user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword!, model.NewPassword!);

            // UpdateSecurityStampAsync => علي الانتهاء علشان يعيد تسجيل الدخول تاني cookie بيجبر ال 
            await _userManager.UpdateSecurityStampAsync(user);

            return changePasswordResult;
        }

        public async Task<ProfileViewModel> GetProfileAsync(string userId, string currentUserId)
        {
            bool isOwner = false;

            ApplicationUser? user;

            var curUserId = _httpContext.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null || userId == currentUserId) // يبقي انا اللي داخل علي صفحتي 
            {
                user = await _userManager.FindByIdAsync(currentUserId);
                isOwner = true;
                userId = currentUserId;
            }
            else
            {
                user = await _userManager.FindByIdAsync(userId);

                if(user is not null)
                    isOwner = false;
            }

            // Post
            var posts = isOwner ?
                await _postService.MyPosts(userId) :
                await _postService.GetAllUserPostsAsync(userId);

            var postsCount = isOwner ?
                await _postService.MyPostsCount(userId) :
                await _postService.VisiblePostsCount(userId);

            // Follow
            var isFollow = await _followService.IsFollowingAsync(userId, currentUserId);
            var followers = await _followService.FollowersCountAsync(userId);
            var followings = await _followService.FollowingCountAsync(userId);

            // Build ViewModel
            return new ProfileViewModel
            {
                UserName = user!.UserName!,
                Email = user.Email!,
                Image = user.ImageURL!,
                BirthDate = user.Birthdate,
                UserId = user.Id,
                IsOwner = isOwner,
                Posts = posts,
                IsFollow = isFollow,
                CountFollowers = followers,
                CountFollowing = followings,
                User = user,
                PostsCount = postsCount
            };
        }
    }
}
