using Blog_Website.Models.Entities;
using Blog_Website.ViewModel.Posts;
using System.ComponentModel.DataAnnotations;

namespace Blog_Website.ViewModel.Profile
{
    public class ProfileViewModel
    {
        [Required]
        public string? UserId { get; set; }

        [Required]
        public string? UserName { get; set; }

        [Required]
        public string? Email { get; set; }

        public DateOnly? BirthDate { get; set; }

        public string? Image { get; set; }

        public bool IsOwner { get; set; }

        public bool IsFollow { get; set; }

        public int CountFollowers { get; set; }
        public int CountFollowing { get; set; }

        public List<PostViewModel> Posts { get; set; } = new List<PostViewModel>();

        public ApplicationUser? User {  get; set; }

        public int PostsCount { get; set; }

    }
}
