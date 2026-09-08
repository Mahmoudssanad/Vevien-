using Blog_Website.Models.Entities;

namespace Blog_Website.ViewModel.Posts
{
    public class DisplayPostViewModel
    {
        public int PostId { get; set; }
        public string? UserId { get; set; }
        public string? ImageUrl { get; set; }
        public string? Content { get; set; }
        public DateTime CreatedDate { get; set; }
        public ApplicationUser? User { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public int TempLikesCount { get; set; }
    }
}
