using Blog_Website.CustomValidation;
using Blog_Website.Enums;
using System.ComponentModel.DataAnnotations;

namespace Blog_Website.ViewModel
{
    public class AccountViewModel
    {
        public string UserName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }

        [BirthdateValidation]
        [DataType(DataType.Date)]
        public DateOnly Birthdate { get; set; }

        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [Compare("ConfirmPassword")]
        public string Password { get; set; }

        [Compare("Password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        [Required]
        public GenderEnum Gender { get; set; }

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
}
