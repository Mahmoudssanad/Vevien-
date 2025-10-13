using Blog_Website.Enums;
using Microsoft.AspNetCore.Identity;

namespace Blog_Website.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public DateOnly Birthdate { get; set; }

        public string? Address { get; set; }

        public string? ImageURL { get; set; } = "~/images/default-profile.png";

        public GenderEnum Gender { get; set; }

        public bool IsDeleted { get; set; }

        public ICollection<Post> Posts { get; set; }
        public ICollection<Like> Likes { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Follow> Followers { get; set; } // اللي بيتابعوني
        public ICollection<Follow> Followings { get; set; } // اللي انا متابعهم
        public ICollection<Notification> Notifications { get; set; }
    }
}
