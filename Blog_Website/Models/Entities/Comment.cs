using System.ComponentModel.DataAnnotations.Schema;

namespace Blog_Website.Models.Entities
{
    public class Comment
    {
        public int Id { get; set; }

        public string? Content { get; set; }

        public string? ImageUrl { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        [ForeignKey("ApplicationUser")]
        public string UserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        [ForeignKey("Post")]
        public int PostId { get; set; }
        public Post Post { get; set; }

        public ICollection<Like> Likes { get; set; }
        public Notification Notification { get; set; }
    }
}
