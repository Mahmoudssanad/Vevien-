using Blog_Website.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Blog_Website.ViewModel.Profile
{
    public class EditViewModel
    {
        public GenderEnum Gender { get; set; }
        public IFormFile Image { get; set; }
        public DateOnly Birthdate { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }

        [HiddenInput]
        public string ImageUrl { get; set; } = "~/images/profile/default.png";
    }
}
