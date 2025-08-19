using System.ComponentModel.DataAnnotations.Schema;

namespace Blog_Website.Models.Entities
{
    public class Like
    {
        public int Id { get; set; }

        public string? Type { get; set; }

        public DateTime CreatedDate { get; set; }

        [ForeignKey("ApplicationUser")]
        public string UserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        [ForeignKey("Post")]
        public int PostId {  get; set; }
        public Post Post { get; set; }

        [ForeignKey("Comment")]
        public int CommentId { get; set; }
        public Comment Comment { get; set; }

        public Notification Notification { get; set; }
    }
}
