using Blog_Website.Models.Entities;

namespace Blog_Website.ViewModel.Comment
{
    public class CommentViewModel
    {
        public string UserId { get; set; }
        public int PostId { get; set; }
        public string? Content { get; set; }
        public string? ImageUrl { get; set; }
        public IFormFile? ImageFile { get; set; }

        public string? UserImageUrl { get; set; }
        public string? UserName { get; set; }
        public DateTime CreatedDate { get; set; }

    }
}
