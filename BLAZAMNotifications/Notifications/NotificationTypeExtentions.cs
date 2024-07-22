using BLAZAM.Database.Models.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Notifications.Notifications
{
    public static class NotificationTypeExtentions
    {
        public static T? ToNotification<T>(this NotificationType type) where T:NotificationTemplateComponent
        {
            NotificationTemplateComponent? notificationTemplate=null; 
            switch (type)
            {
                case NotificationType.PasswordChange:
                    notificationTemplate = new PasswordChangedEmailMessage();
                    break;
            }
            if(notificationTemplate != null)
            {
                return (T?)notificationTemplate;    
            }
            return default(T);
        }
    }
}
