using Blog_Website.Enums;
using Microsoft.AspNetCore.Identity;

namespace Blog_Website.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public DateOnly Birthdate { get; set; }

        public string? Address { get; set; }

        public string? ImageURL { get; set; }

        public GenderEnum Gender { get; set; }

        public bool IsDeleted { get; set; }

        public ICollection<DesplayPostViewModel> Posts { get; set; } = new List<DesplayPostViewModel>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        // اللي بيتابعوني
        public ICollection<Follow> Followers { get; set; } = new List<Follow>();

        // اللي انا متابعهم
        public ICollection<Follow> Followings { get; set; } = new List<Follow>();

        public ICollection<Notification> ReceivedNotifications { get; set; } = new List<Notification>();

        public ICollection<Notification> SentNotifications { get; set; } = new List<Notification>();
    }
}
