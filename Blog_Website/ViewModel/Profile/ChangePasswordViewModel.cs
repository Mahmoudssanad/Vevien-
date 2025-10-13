using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Blog_Website.ViewModel.Profile
{
    public class ChangePasswordViewModel
    {
        [DataType(DataType.Password)]
        [DisplayName("Current Password")]
        public string CurrentPassword { get; set; }

        [MaxLength(32, ErrorMessage = "must be greater than 8 and less than or equal 32")]
        [MinLength(8, ErrorMessage = "must be greater than 8 and less than or equal 32")]
        [DataType(DataType.Password)]
        [DisplayName("New Password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword")]
        [DisplayName("Confirm Password")]
        public string ConfirmPassword { get; set; }
    }
}
