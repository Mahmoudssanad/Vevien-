using Blog_Website.Models.Entities;
using Blog_Website.Services.IServices;
using Blog_Website.ViewModel.Account;
using Microsoft.AspNetCore.Identity;

namespace Blog_Website.Services
{
    public class AccountService(UserManager<ApplicationUser> _userManager) : IAccountService
    {
        // Mapping and Create (Separate of concerns)
        public async Task<(bool Succeeded, ApplicationUser? User)> CreateAsync(AccountViewModel data)
        {
            var user = new ApplicationUser
            {
                UserName = data.UserName,
                Email = data.Email,
                Address = data.Address,
                Gender = data.Gender,
                Birthdate = data.Birthdate,
                PhoneNumber = data.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, data.Password!);

            return(result.Succeeded,  result.Succeeded ? user : null);
        }
    }
}
