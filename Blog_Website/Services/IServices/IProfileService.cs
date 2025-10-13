using Blog_Website.Models.Entities;
using Blog_Website.ViewModel.Profile;

namespace Blog_Website.Services.IServices
{
    public interface IProfileService
    {
        Task ChangePassword(ChangePasswordViewModel model);

        Task DeleteAsync(string userId);

        Task<List<ProfileViewModel>> GetAllAsync();
    }
}
