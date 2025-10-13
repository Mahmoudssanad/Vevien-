using System.ComponentModel.DataAnnotations;

namespace Blog_Website.ViewModel.Account
{
    public class ResetPasswordViewModel
    {
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [StringLength(100,ErrorMessage = "The password must be at least {2} characters.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string Password { get; set; }

        [Compare("Password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
    }
}
