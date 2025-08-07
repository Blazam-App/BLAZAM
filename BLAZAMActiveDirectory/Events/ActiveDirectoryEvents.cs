using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.ActiveDirectory.Events
{
    public static class ActiveDirectoryEvents
    {
        /// <summary>
        /// Called when a user logs in or out of the application
        /// </summary>
        public static AppEvent<int> LoggedOnUserCountChanged { get; set; } = new();
    }
}
