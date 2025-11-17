using Blog_Website.Enums;
using System.ComponentModel.DataAnnotations;

namespace Blog_Website.ViewModel.Account
{
    public class OtpViewModel
    {
        [Required]
        public string? Email { get; set; }

        [Required]
        public string? Otp { get; set; }

        public OtpFlow Flow { get; set; }
    }
}
