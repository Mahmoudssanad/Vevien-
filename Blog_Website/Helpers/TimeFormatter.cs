namespace Blog_Website.Helpers
{
    public class TimeFormatter
    {
        public static string ToInstagramStyle(DateTime createdDate)
        {
            var now = DateTime.UtcNow;
            var diff = now - createdDate.ToUniversalTime();

            if (diff.TotalSeconds < 60)
                return $"{(int)diff.TotalSeconds}s ago";
            else if (diff.TotalMinutes < 60)
                return $"{(int)diff.TotalMinutes}m ago";
            else if (diff.TotalHours < 24)
                return $"{(int)diff.TotalHours}h ago";
            else if (diff.TotalDays < 2)
                return "Yesterday";
            else if (diff.TotalDays < 7)
                return $"{(int)diff.TotalDays}d ago";
            else
                return createdDate.ToLocalTime().ToString("MMM dd");
        }
    }
}
