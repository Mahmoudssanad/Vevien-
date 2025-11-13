using Blog_Website.CustomValidation;
using Blog_Website.Enums;
using System.ComponentModel.DataAnnotations;

namespace Blog_Website.ViewModel.Account
{
    public class AccountViewModel
    {
        [Required]
        [MaxLength(250)]
        [MinLength(3, ErrorMessage = "Name must be greater than or equal 3 char")]
        public string? UserName { get; set; }


        [Required]
        [MaxLength(2500)]
        [MinLength(4, ErrorMessage ="Address must be greater than 3 char")]
        public string? Address { get; set; }


        [Required]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "Phone number must be only 11 number without spaces or char")]
        public string? PhoneNumber { get; set; }


        [BirthdateValidation]
        [DataType(DataType.Date)]
        [Required]
        public DateOnly Birthdate { get; set; }


        [DataType(DataType.EmailAddress)]
        [Required]
        public string? Email { get; set; }


        [Required]
        public string? Password { get; set; }


        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string? ConfirmPassword { get; set; }


        [Required]
        public GenderEnum Gender { get; set; }


        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
}
