using Blog_Website.Models.Entities;
using Blog_Website.ViewModel.Account;
using Microsoft.AspNetCore.Identity;

namespace Blog_Website.Services.IServices
{
    public interface IAccountService
    {
        Task<(bool Succeeded, ApplicationUser? User)> CreateAsync(AccountViewModel data);

        //Task<IdentityResult> RegisterUserAsync(AccountViewModel model);
    }
}
