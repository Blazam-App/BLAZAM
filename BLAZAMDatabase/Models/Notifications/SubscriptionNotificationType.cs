using BLAZAM.Database.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Database.Models.Notifications
{
    public class SubscriptionNotificationType : AppDbSetBase
    {
        public NotificationSubscription NotificationSubscription { get; set; }
        public NotificationType NotificationType { get; set; }
    }
}
