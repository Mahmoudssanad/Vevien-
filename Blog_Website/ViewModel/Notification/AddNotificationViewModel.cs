using System.ComponentModel.DataAnnotations;

namespace Blog_Website.ViewModel.Notification
{
    public class AddNotificationViewModel
    {
        [Required]
        public string? SenderId { get; set; }

        [Required]
        public string? ReceiverId { get; set; }

        [Required]
        public string? Title { get; set; }

        [Required]
        public string? Description { get; set; }

        [Required]
        public string? RedirectUrl { get; set; }

        [Required]
        public string? Type { get; set; }

        public int TargetId { get; set; }
    }
}
