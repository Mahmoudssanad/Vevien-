using Blog_Website.Models.Entities;
using Blog_Website.Services.IServices;
using Blog_Website.ViewModel.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Blog_Website.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProfileService _profileService;
        private readonly IImageService _imageService;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            IProfileService profileService,
            IPostService postService,
            IFollowService followService,
            IImageService imageService
            )
        {
            _userManager = userManager;
            _profileService = profileService;
            _imageService = imageService;
        }

        [HttpGet]
        public async Task<IActionResult> Profile(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
                return RedirectToAction("Login", "Account");

            var profile = await _profileService.GetProfileAsync(userId, currentUser.Id);

            if (profile == null)
                return NotFound();

            return View(profile);
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
            if (user is null)
                return Unauthorized();

            if (ModelState.IsValid)
            {
                var imagePath = user!.ImageURL;

                // upload image
                if(model.Image != null)
                {
                    var oldImage = model.ImageUrl;

                    var newImagePath = await _imageService.UploadProfileImageAsync(model.Image);

                    // Update image
                    user.ImageURL = newImagePath;

                    // delete old image
                    _imageService.DeleteImage(oldImage!);
                }

                user.Birthdate = model.Birthdate;
                user.PhoneNumber = model.Phone;
                user.Gender = model.Gender;
                user.Address = model.Address;

                var result = await _userManager.UpdateAsync(user);

                if(result.Succeeded)
                    return RedirectToAction("Profile", new {userId = user.Id});

                foreach (var error in result.Errors)
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
                return RedirectToAction("Login", "Account");
            }

            foreach(var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            var existingUser = await _userManager.FindByIdAsync(user!.Id);
            if (user is null)
                return NotFound();

            var result = await _profileService.SoftDeleteUserAsync(user.Id);

            return result ? Json(new { success = true }) : Json(new { success = false });
        }
    }
}
