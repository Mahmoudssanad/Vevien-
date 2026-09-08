using System.ComponentModel.DataAnnotations;

namespace Blog_Website.ViewModel.Account
{
    public class ForgetPasswordViewModel
    {
        [DataType(DataType.EmailAddress)]
        [Required]
        public string? Email { get; set; }
    }
}
