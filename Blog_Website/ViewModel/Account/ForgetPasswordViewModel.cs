using System.ComponentModel.DataAnnotations;

namespace Blog_Website.ViewModel.Account
{
    public class ForgetPasswordViewModel
    {
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}
