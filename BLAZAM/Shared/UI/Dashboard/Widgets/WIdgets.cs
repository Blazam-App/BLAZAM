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
                    widgets.Add(new LockedOutUsers() { Slot = "slot1" , Name="Locked Out Users"});
                if (applicationUser.IsSuperAdmin || applicationUser.HasRole(UserRoles.SearchUsers))
    
                    widgets.Add(new NewUsersWidget() { Slot = "slot1",Name = "Users created in the last 14 days" }); 

                if (applicationUser.IsSuperAdmin || applicationUser.HasRole(UserRoles.SearchOUs))
    
                    widgets.Add(new NewOUsWidget() { Slot = "slot1",Name = "OU's created in the last 14 days" });
                if (applicationUser.IsSuperAdmin || applicationUser.HasRole(UserRoles.SearchGroups))
    
                    widgets.Add(new NewGroupsWidget() { Slot = "slot2",Name = "Groups created in the last 14 days" });
                if (applicationUser.IsSuperAdmin || applicationUser.HasRole(UserRoles.SearchComputers))
    
                    widgets.Add(new NewComputersWidget() { Slot = "slot2",Name = "Computers created in the last 14 days" });
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

