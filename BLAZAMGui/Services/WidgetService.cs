using BLAZAM.Database.Models.User;
using BLAZAM.Gui.UI.Dashboard.Widgets;
using BLAZAM.Gui.UI.Dashboard.Widgets.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Gui.Services
{
    /// <summary>
    /// A service to provide the appropriate widgets to users, based on permissions
    /// </summary>
    public class WidgetService
    {
        private ICurrentUserStateService _currentUserStateService;
        private IStringLocalizer<AppLocalization> AppLocalization;

        public WidgetService(ICurrentUserStateService currentUserStateService, IStringLocalizer<AppLocalization> appLocalization)
        {
            _currentUserStateService = currentUserStateService;

            AppLocalization = appLocalization;
        }
        public List<Widget> Available()
        {
            var applicationUser = _currentUserStateService.State;
            var widgets = new List<Widget>();
            if (applicationUser != null)
            {
                if (applicationUser.IsSuperAdmin || applicationUser.CanUnlockUsers)
                    widgets.Add(new LockedOutUsers() { WidgetType = DashboardWidgetType.LockedOutUsers, Title = AppLocalization["Locked Out Users"] });
                if (applicationUser.IsSuperAdmin || applicationUser.HasRole(UserRoles.SearchUsers))

                    widgets.Add(new NewUsersWidget() { WidgetType = DashboardWidgetType.NewUsers, Title = AppLocalization["Users created in the last 14 days"] });

                if (applicationUser.IsSuperAdmin || applicationUser.HasRole(UserRoles.SearchUsers)
                    || applicationUser.HasRole(UserRoles.SearchOUs)
                     || applicationUser.HasRole(UserRoles.SearchGroups)
                     || applicationUser.HasRole(UserRoles.SearchPrinters)
                      || applicationUser.HasRole(UserRoles.SearchComputers))
                    widgets.Add(new ChangedEntriesWidget() { WidgetType = DashboardWidgetType.ChangedEntries, Title = AppLocalization["Entries changed in the last 24 hours"] });


                if (applicationUser.IsSuperAdmin || applicationUser.HasRole(UserRoles.SearchOUs))

                    widgets.Add(new NewOUsWidget() { WidgetType = DashboardWidgetType.NewOus, Title = AppLocalization["OU's created in the last 14 days"] });
                if (applicationUser.IsSuperAdmin || applicationUser.HasRole(UserRoles.SearchGroups))

                    widgets.Add(new NewGroupsWidget() { WidgetType = DashboardWidgetType.NewGroups, Title = AppLocalization["Groups created in the last 14 days"] });

                if (applicationUser.IsSuperAdmin || applicationUser.HasRole(UserRoles.SearchPrinters))

                    widgets.Add(new NewPrintersWidget() { WidgetType = DashboardWidgetType.NewPrinters, Title = AppLocalization["Printers created in the last 14 days"] });

                if (applicationUser.IsSuperAdmin || applicationUser.HasRole(UserRoles.SearchComputers))

                    widgets.Add(new NewComputersWidget() { WidgetType = DashboardWidgetType.NewComputers, Title = AppLocalization["Computers created in the last 14 days"] });
                if (applicationUser.IsSuperAdmin)
                {
                    widgets.Add(new ChangedPasswordsWidget() { WidgetType = DashboardWidgetType.PasswordsChanged, Title = "Password Changed" });
                    widgets.Add(new DeletedEntriesWidget() { WidgetType = DashboardWidgetType.DeletedEntries, Title = AppLocalization["Entries deleted in the last 14 days"] });
                    widgets.Add(new AppLogonsWidget() { WidgetType = DashboardWidgetType.AppLogons, Title = AppLocalization["Application logons"] });

                }

                widgets.Add(new FavoritesWidget() { WidgetType = DashboardWidgetType.FavoriteEntries, Title = AppLocalization["Favorites"] });

            }
            return widgets;
        }

    }
}
