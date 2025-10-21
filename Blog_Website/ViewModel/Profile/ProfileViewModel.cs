using Blog_Website.Models.Entities;
using Blog_Website.ViewModel.Post;

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

        public bool IsFollow { get; set; }

        public int CountFollowers { get; set; }
        public int CountFollowing { get; set; }

        public List<PostViewModel> Posts {  get; set; }

        public ApplicationUser User {  get; set; }

        public int PostsCount { get; set; }

    }
}
