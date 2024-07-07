using BLAZAM.Database.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Database.Models.Notifications
{
    public class NotificationSubscription:AppDbSetBase
    {
        public AppUser User { get; set; }
        public NotificationType NotificationType { get; set; }
        public string OU { get; set; }
        public bool Block { get; set; } = false;
    }
}
