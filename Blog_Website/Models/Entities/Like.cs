using Blog_Website.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blog_Website.Models.Entities
{
    public class Like
    {
        public int Id { get; set; }

        public DateTime CreatedDate { get; set; }


        [ForeignKey("ApplicationUser")]
        public string? UserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }

        // بدل ما نربط بـ PostId فقط، نخلي العلاقة Polymorphic
        public int TargetId { get; set; } // ممكن تكون Id البوست أو Id الكومنت
        public LikeTargetType TargetType { get; set; } // نوع الكيان اللي متعمل عليه لايك
    }
}
