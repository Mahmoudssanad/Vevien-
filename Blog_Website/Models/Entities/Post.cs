using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blog_Website.Models.Entities
{
    public class Post
    {
        public int Id { get; set; }

        [DataType(DataType.Text)]
        [MaxLength(1000, ErrorMessage = "must be less or equal 1000 word")]
        public string? Content { get; set; }

        public string? ImageUrl {  get; set; }

        public bool Visible {  get; set; }

        public bool Public {  get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime UpdateDate { get; set; }

        [ForeignKey("ApplicationUser")]
        public string UserId {  get; set; }
        public ApplicationUser ApplicationUser {  get; set; }

        public ICollection<Like> Likes {  get; set; }
        public ICollection<Comment> Comments {  get; set; }
    }
}
