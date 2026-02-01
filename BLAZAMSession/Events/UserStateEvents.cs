using BLAZAM.Session.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Session.Events
{
    public static class UserStateEvents
    {
        /// <summary>
        /// Called when a user logs in  
        /// </summary>
        public static AppEvent<IApplicationUserState> UserLoggedIn { get; set; } = new();

        /// <summary>
        /// Called when a user logs out  
        /// </summary>
        public static AppEvent<IApplicationUserState> UserLoggedOut { get; set; } = new();

        /// <summary>
        /// Called when a user's session times out 
        /// </summary>
        public static AppEvent<IApplicationUserState> UserTimedOut { get; set; } = new();


    }
}
