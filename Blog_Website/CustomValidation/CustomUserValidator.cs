using Blog_Website.Models.Entities;
using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;

namespace Blog_Website.CustomValidation
{
    public class CustomUserValidator : IUserValidator<ApplicationUser>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
        {
            var errors = new List<IdentityError>();

            var regex = new Regex(@"^[\u0600-\u06FFa-zA-Z0-9 _.-]+$");

            if (!regex.IsMatch(user.UserName!))
            {
                errors.Add(new IdentityError
                {
                    Code = "InvalidUserName",
                    Description = "UserName only contain some char of arabic or english and _-."
                });
            }

            if (errors.Any())
                return Task.FromResult(IdentityResult.Failed(errors.ToArray()));

            return Task.FromResult(IdentityResult.Success);
        }
    }
}
