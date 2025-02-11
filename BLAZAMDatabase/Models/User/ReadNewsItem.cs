namespace BLAZAM.Database.Models.User
{
    public class ReadNewsItem : AppDbSetBase
    {
        public AppUser User { get; set; }
        public double NewsItemId { get; set; }
        public DateTime NewsItemUpdatedAt { get; set; }
    }
}
