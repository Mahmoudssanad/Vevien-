using System.ComponentModel.DataAnnotations;

namespace Blog_Website.ViewModel.Comments
{
    public class CommentViewModel
    {
        [Required]
        public string? UserId { get; set; }
        public int PostId { get; set; }
        public string? Content { get; set; }
        public string? ImageUrl { get; set; }
        public IFormFile? ImageFile { get; set; }

        public string? UserImageUrl { get; set; }
        public string? UserName { get; set; }
        public DateTime CreatedDate { get; set; }

    }
}
