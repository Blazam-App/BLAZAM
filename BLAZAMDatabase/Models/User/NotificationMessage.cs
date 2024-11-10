
using BLAZAM.Database.Models.Permissions;

namespace BLAZAM.Database.Models.User
{

    public enum NotificationLevel { Info, Success, Warning, Error }
    public enum MessageType { Notification, AccessRequest }
    /// <summary>
    /// A notification message for the web user. These are
    /// placed under the user's notifications panel
    /// </summary>
    public class NotificationMessage : AppDbSetBase
    {

        /// <summary>
        /// The severity level of this notification
        /// </summary>
        public NotificationLevel Level { get; set; }

        /// <summary>
        /// The message type of this notification
        /// </summary>
        public MessageType MessageType { get; set; }  = MessageType.Notification;

        /// <summary>
        /// The DN of the target for this notification
        /// </summary>
        public string? TargetDN { get; set; }

        /// <summary>
        /// The action being requested access to
        /// </summary>
        public ActiveDirectoryObjectAction? Action { get; set; }

        /// <summary>
        /// The user who triggered this notification
        /// </summary>
        public AppUser? Creator { get; set; }
        public int? CreatorId { get; set; }

        /// <summary>
        /// The title of the notification
        /// </summary>
        public string? Title { get; set; }

        
        /// <summary>
        /// The message of the notification
        /// </summary>
        public string? Message { get; set; }

        public Uri? Link { get; set; }
        /// <summary>
        /// If  set, the message will be deleted after this <see cref="DateTime"/>
        /// </summary>
        public DateTime? Expires { get; set; }

        public DateTime Created { get; set; } = DateTime.Now;

        public bool Dismissable { get; set; } = true;

        /// <summary>
        /// True if the Id's match or the <see cref="Level"/>, 
        /// <see cref="Title"/>, and <see cref="Message"/> match
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {

            return obj is NotificationMessage message && (
                   Id != 0 && Id == message.Id);
        }
        
    }
}
