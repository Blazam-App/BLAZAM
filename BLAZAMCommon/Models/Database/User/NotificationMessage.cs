namespace BLAZAM.Common.Models.Database.User
{

    public enum NotificationLevel { Info, Success, Warning, Error }
    /// <summary>
    /// A notification message for the web user. These are
    /// placed under the user's notifications panel
    /// </summary>
    public class NotificationMessage
    {
        public int Id { get; set; }

        /// <summary>
        /// The severity level of this notification
        /// </summary>
        public NotificationLevel Level { get; set; }
        /// <summary>
        /// The title of the notificaiton
        /// </summary>
        public string? Title { get; set; }
        /// <summary>
        /// The message of the notificatioon
        /// </summary>
        public string? Message { get; set; }
        public Uri? Link { get; set; }
        /// <summary>
        /// If  set, the message will be deleted after this <see cref="DateTime"/>
        /// </summary>
        public DateTime? Expires { get; set; }

        public DateTime? Created { get; set; }

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
