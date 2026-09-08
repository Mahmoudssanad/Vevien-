namespace Blog_Website.Models.Entities
{
    public class OTP
    {
        public int Id { get; set; }
        
        public string Email { get; set; }

        public string Code { get; set; }

        public DateTime ExpiryTime { get; set; }

        public bool IsUsed { get; set; }
    }
}
