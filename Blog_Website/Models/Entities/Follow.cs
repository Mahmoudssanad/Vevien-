using System.ComponentModel.DataAnnotations.Schema;

namespace Blog_Website.Models.Entities
{
    public class Follow
    {
        public int Id { get; set; }

        public DateTime CreatedDate { get; set; }

        [ForeignKey("Follower")]
        public string FollowerId { get; set; }
        public ApplicationUser Follower { get; set; }

        [ForeignKey("Following")]
        public string FollowingId { get; set; }
        public ApplicationUser Following { get; set; }

        public Notification Notification { get; set; }

    }
}
