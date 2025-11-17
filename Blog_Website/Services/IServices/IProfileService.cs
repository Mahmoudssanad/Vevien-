using Blog_Website.ViewModel.Profile;
using Microsoft.AspNetCore.Identity;

namespace Blog_Website.Services.IServices
{
    public interface IProfileService
    {
        Task<IdentityResult> ChangePassword(ChangePasswordViewModel model);

        Task<bool> SoftDeleteUserAsync(string userId);

        Task<List<ProfileViewModel>> GetAllAsync();

        Task<ProfileViewModel> GetProfileAsync(string userId, string CurrentUserId);
    }
}
