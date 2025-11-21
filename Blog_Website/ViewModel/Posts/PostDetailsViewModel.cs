using Blog_Website.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace Blog_Website.ViewModel.Posts
{
    public class PostDetailsViewModel
    {
        public int PostId { get; set; }

        [Required]
        public string? UserId { get; set; }
        public string? ImageUrl { get; set; }
        public string? Content { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public int TempLikesCount { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public ApplicationUser? User { get; set; }

        public bool Public { get; set; }
        public bool Visible { get; set; }
    }
}
