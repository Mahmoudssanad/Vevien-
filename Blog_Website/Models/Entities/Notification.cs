using System.ComponentModel.DataAnnotations.Schema;

namespace Blog_Website.Models.Entities
{
    public class Notification
    {
        public int Id { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? RedirectUrl { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedDate { get; set; }

        [ForeignKey("ApplicationUser")]
        public string? ReceiverId { get; set; }
        public ApplicationUser? Receiver { get; set; }

        public string? SenderId { get; set; }
        public ApplicationUser? Sender { get; set; }

        // null عندي ب rows دي تغني عن كل العلاقات اللي انا عاملها تحت دي لان هي بتحدد النوع علشان ميبقاش معظم ال property المفروض ال 
        // Polymorphic relationship  نفس فكره ال 
        public string? Type { get; set; }
        public int TargetId { get; set; } = 1;

    }
}
