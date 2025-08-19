using System.ComponentModel.DataAnnotations.Schema;

namespace Blog_Website.Models.Entities
{
    public class Notification
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string RedirectUrl { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedDate { get; set; }

        [ForeignKey("ApplicationUser")]
        public string UserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        [ForeignKey("Like")]
        public int LikeId {  get; set; }
        public Like Like { get; set; }

        [ForeignKey("Comment")]
        public int CommentId { get; set; }
        public Comment Comment { get; set; }

        [ForeignKey("Follow")]
        public int FollowId { get; set; }
        public Comment Follow { get; set; }
    }
}
