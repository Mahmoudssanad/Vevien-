using Blog_Website.Models.Data;
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

        public ProfileService(UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContext)
        {
            _userManager = userManager;
            _httpContext = httpContext;
        }
        public async Task DeleteAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<ProfileViewModel>> GetAllAsync()
        {
            var currentUserId = _httpContext.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var allUsers = await _userManager.Users.Where(x => x.Id != currentUserId).ToListAsync();

            return allUsers.Select(u => new ProfileViewModel
            {
                Email = u.Email,
                UserName = u.UserName,
                UserId = u.Id,
                Image = u.ImageURL,
            }).ToList();
        }

        public async Task<IdentityResult> ChangePassword(ChangePasswordViewModel model)
        {
            var userId = _httpContext.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _userManager.FindByIdAsync(userId);

            if(user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            return changePasswordResult;
        }
    }
}
