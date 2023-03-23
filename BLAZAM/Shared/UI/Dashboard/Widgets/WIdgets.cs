using BLAZAM.Common.Data;
using BLAZAM.Common.Data.Services;
using BLAZAM.Server.Data.Services;
using Microsoft.AspNetCore.Components;

namespace BLAZAM.Server.Shared.UI.Dashboard.Widgets
{
    public class Widgets
    { 
        public static List<Widget> Available(IApplicationUserState? applicationUser)
        {
            var widgets = new List<Widget>();
            if (applicationUser != null)
            {
                if (applicationUser.IsSuperAdmin || applicationUser.DirectoryUser.CanUnlock)
                    widgets.Add(new LockedOutUsers() { Slot = "slot1" });
                if (applicationUser.IsSuperAdmin || applicationUser.HasRole(UserRoles.SearchUsers))
    
                    widgets.Add(new NewUsersWidget() { Slot = "slot2",Name = "Users created in the last 90 days" });
                if (applicationUser.IsSuperAdmin)
                    widgets.Add(new ChangedPasswordsWidget() { Slot = "slot1" });
            }
            return widgets;
        }
        /*
        public static List<Widget> Selected(IApplicationUserState? applicationUser)
        {
            //var selected = applicationUser.UserSettings.Widgets;
        }
        */
    }
}

