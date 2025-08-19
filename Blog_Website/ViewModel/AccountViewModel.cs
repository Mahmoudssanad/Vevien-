using Blog_Website.CustomValidation;
using Blog_Website.Enums;
using System.ComponentModel.DataAnnotations;

namespace Blog_Website.ViewModel
{
    public class AccountViewModel
    {
        public string UserName { get; set; }
        public string Address { get; set; }

        [BirthdateValidation]
        public DateOnly Birthdate { get; set; }

        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        public GenderEnum Gender { get; set; }
    }
}
