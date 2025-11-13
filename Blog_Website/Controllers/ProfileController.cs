using Blog_Website.Models.Entities;
using Blog_Website.Services.IServices;
using Blog_Website.ViewModel.Profile;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Blog_Website.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHost;
        private readonly IProfileService _profileService;
        private readonly IPostService _postService;
        private readonly IFollowService _followService;

        public ProfileController(UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHost, IProfileService profileService, IPostService postService, IFollowService followService)
        {
            _userManager = userManager;
            _webHost = webHost;
            _profileService = profileService;
            _postService = postService;
            _followService = followService;
        }

        [HttpGet]
        public async Task<IActionResult> Profile(string userId)
        {
            ApplicationUser user;
            bool flag = false;

            if (string.IsNullOrEmpty(userId))
            {
                user = await _userManager.GetUserAsync(User);
                flag = true;
            }
            else
                user = await _userManager.FindByIdAsync(userId);

            var userPosts = await _postService.GetAllUserPostsAsync(userId);

            var postsCount = await _postService.VisiblePostsCount(userId);

            if (userId == User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                flag = true;
                userPosts = await _postService.MyPosts(userId);
                postsCount = await _postService.MyPostsCount(userId);
            }

            if (user == null)
                return NotFound();

            var currentUserId = _userManager.GetUserId(User);

            var isFollow = await _followService.IsFollowingAsync(userId, currentUserId!);

            var followersCount = await _followService.FollowersCountAsync(userId);
            var followingsCount = await _followService.FollowingCountAsync(userId);


            var userProfile = new ProfileViewModel
                {
                    UserName = user.UserName!,
                    Email = user.Email!,
                    Image = user.ImageURL!,
                    BirthDate = user.Birthdate,
                    UserId = user.Id,
                    IsOwner = flag,
                    Posts = userPosts,
                    IsFollow = isFollow,
                    CountFollowers = followersCount,
                    CountFollowing = followingsCount,
                    User = user,
                    PostsCount = postsCount
                };


            return View(userProfile);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null) return NotFound();

            var userProfileEdit = new EditViewModel
            {
                Address = user.Address,
                ImageUrl = user.ImageURL,
                Birthdate = user.Birthdate,
                Gender = user.Gender,
                Phone = user.PhoneNumber
            };

            return View(userProfileEdit);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Edit(EditViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            if (ModelState.IsValid)
            {
                var imagePath = user!.ImageURL;

                if(model.Image != null)
                {
                    var uploadFolder = Path.Combine(_webHost.WebRootPath, "images/profile");

                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}";

                    var fullPath = Path.Combine(uploadFolder, fileName);

                    using(Stream stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await model.Image.CopyToAsync(stream);
                    }

                    if (!string.IsNullOrEmpty(user.ImageURL) && user.ImageURL != "/images/profile/default.png")
                    {
                        var oldImagePath = Path.Combine(_webHost.WebRootPath, $"images/profile/{user.ImageURL}");
                        if (System.IO.File.Exists(oldImagePath))
                            System.IO.File.Delete(oldImagePath);
                    }


                    imagePath = $"/images/profile/{fileName}";
                }

                user.ImageURL = imagePath;
                user.Birthdate = model.Birthdate;
                user.PhoneNumber = model.Phone;
                user.Gender = model.Gender;
                user.Address = model.Address;

                var saveResultInDatabase = await _userManager.UpdateAsync(user);

                if(saveResultInDatabase.Succeeded)
                    return RedirectToAction("Profile", new {userId = userId});

                foreach (var error in saveResultInDatabase.Errors)
                    ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var allUsers = await _profileService.GetAllAsync();

            return View(allUsers);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await _profileService.ChangePassword(model);

            if (result.Succeeded)
            {
                TempData["Message"] = "Password changed successfully.";
                return RedirectToAction("Login", "Account");
            }

            foreach(var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }
    }
}
