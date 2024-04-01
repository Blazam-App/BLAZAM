

using BLAZAM.Database.Models.User;

namespace BLAZAM.Gui.UI.Dashboard.Widgets
{
    public class AllWidgets
    {
        public static List<Widget> Available(IApplicationUserState? applicationUser)
        {
            var widgets = new List<Widget>();
            if (applicationUser != null)
            {
                if (applicationUser.IsSuperAdmin || applicationUser.CanUnlockUsers)
                    widgets.Add(new LockedOutUsers() { WidgetType = DashboardWidgetType.LockedOutUsers, Title = "Locked Out Users" });
                if (applicationUser.IsSuperAdmin || applicationUser.HasRole(UserRoles.SearchUsers))

                    widgets.Add(new NewUsersWidget() { WidgetType = DashboardWidgetType.NewUsers, Title = "Users created in the last 14 days" });

                if (applicationUser.IsSuperAdmin || applicationUser.HasRole(UserRoles.SearchUsers)
                    || applicationUser.HasRole(UserRoles.SearchOUs)
                     || applicationUser.HasRole(UserRoles.SearchGroups)
                     || applicationUser.HasRole(UserRoles.SearchPrinters)
                      || applicationUser.HasRole(UserRoles.SearchComputers))
                    widgets.Add(new ChangedEntriesWidget() { WidgetType = DashboardWidgetType.ChangedEntries, Title = "Entries changed in the last 24 hhours" });


                if (applicationUser.IsSuperAdmin || applicationUser.HasRole(UserRoles.SearchOUs))

                    widgets.Add(new NewOUsWidget() { WidgetType = DashboardWidgetType.NewOus, Title = "OU's created in the last 14 days" });
                if (applicationUser.IsSuperAdmin || applicationUser.HasRole(UserRoles.SearchGroups))

                    widgets.Add(new NewGroupsWidget() { WidgetType = DashboardWidgetType.NewGroups, Title = "Groups created in the last 14 days" });

                if (applicationUser.IsSuperAdmin || applicationUser.HasRole(UserRoles.SearchPrinters))

                    widgets.Add(new NewPrintersWidget() { WidgetType = DashboardWidgetType.NewPrinters, Title = "Printers created in the last 14 days" });

                if (applicationUser.IsSuperAdmin || applicationUser.HasRole(UserRoles.SearchComputers))

                    widgets.Add(new NewComputersWidget() { WidgetType = DashboardWidgetType.NewComputers, Title = "Computers created in the last 14 days" });
                if (applicationUser.IsSuperAdmin)
                {
                    widgets.Add(new ChangedPasswordsWidget() { WidgetType = DashboardWidgetType.PasswordsChanged, Title = "Password Changed" });
                    widgets.Add(new DeletedEntriesWidget() { WidgetType = DashboardWidgetType.DeletedEntries, Title = "Entries deleted in the last 14 days" });
                }

                widgets.Add(new FavoritesWidget() { WidgetType = DashboardWidgetType.FavoriteEntries, Title = "Favorites" });

            }
            return widgets;
        }

    }
}

