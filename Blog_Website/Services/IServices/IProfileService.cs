using Blog_Website.Models.Entities;
using Blog_Website.ViewModel.Profile;
using Microsoft.AspNetCore.Identity;

namespace Blog_Website.Services.IServices
{
    public interface IProfileService
    {
        Task<IdentityResult> ChangePassword(ChangePasswordViewModel model);

        Task DeleteAsync(string userId);

        Task<List<ProfileViewModel>> GetAllAsync();
    }
}
