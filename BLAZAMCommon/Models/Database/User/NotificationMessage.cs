namespace BLAZAM.Common.Models.Database.User
{
    public enum NotificationLevel { Info, Success, Warning, Error }
    public class NotificationMessage
    {
        public int Id { get; set; }
        public NotificationLevel Level { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }
        public Uri? Link { get; set; }
        /// <summary>
        /// If  set, the message will be deleted after this <see cref="DateTime"/>
        /// </summary>
        public DateTime? Expires { get; set; }

        public bool Dismissable { get; set; } = true;

        /// <summary>
        /// True if the Id's match or the <see cref="Level"/>, 
        /// <see cref="Title"/>, and <see cref="Message"/> match
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            
            return obj is NotificationMessage message &&(
                   (Id!=0  && Id == message.Id) ||
                   (Level == message.Level &&
                   Title == message.Title &&
                   Message == message.Message));
        }
    }
}
