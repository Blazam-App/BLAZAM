using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Services.Notifications
{
    public class NotificationService
    {
        private AppDatabaseFactory _databaseFactory;

        public NotificationService(AppDatabaseFactory databaseFactory)
        {
            _databaseFactory = databaseFactory;
        }

        public void Post(IDirectoryEntryAdapter source, NotificationType notificationType)
        {

        }

    }
}
