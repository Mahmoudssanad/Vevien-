namespace Blog_Website.ViewModel.Profile
{
    public class ProfileViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public DateOnly? BirthDate { get; set; }
        public string Image { get; set; } = "~/images/profile/default.png";
        public bool IsOwner { get; set; }
    }
}
