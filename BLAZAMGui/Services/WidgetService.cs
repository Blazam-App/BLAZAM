using BLAZAM.Database.Models.User;
using BLAZAM.Gui.UI.Dashboard.Widgets;
using BLAZAM.Gui.UI.Dashboard.Widgets.Admin;

namespace BLAZAM.Gui.Services
{
    /// <summary>
    /// A service to provide the appropriate widgets to users, based on permissions
    /// </summary>
    public class WidgetService
    {
        private readonly ICurrentUserStateService _currentUserStateService;
        private readonly IStringLocalizer<AppLocalization> AppLocalization;

        public WidgetService(ICurrentUserStateService currentUserStateService, IStringLocalizer<AppLocalization> appLocalization)
        {
            _currentUserStateService = currentUserStateService;

            AppLocalization = appLocalization;
        }
        public List<Widget> Available()
        {
            var applicationUser = _currentUserStateService.State;
            var widgets = new List<Widget>();
            if (applicationUser == null)
                return widgets;

            bool isSuperAdmin = applicationUser.IsSuperAdmin;

            void AddWidgetIf(bool condition, Widget widget)
            {
                if (condition)
                    widgets.Add(widget);
            }

            AddWidgetIf(isSuperAdmin || applicationUser.CanUnlockUsers,
                new LockedOutUsers() { WidgetType = DashboardWidgetType.LockedOutUsers, Title = AppLocalization[Lang.Locked_Out_Users] });

            AddWidgetIf(isSuperAdmin || applicationUser.HasRole(UserRoles.SearchUsers),
                new NewUsersWidget() { WidgetType = DashboardWidgetType.NewUsers, Title = AppLocalization["Users created in the last 14 days"] });

            AddWidgetIf(isSuperAdmin || applicationUser.HasRole(UserRoles.SearchUsers),
                new ChangedPasswordsWidget() { WidgetType = DashboardWidgetType.PasswordsChanged, Title = AppLocalization["Passwords changed in the last 90 days"] });

            AddWidgetIf(isSuperAdmin || applicationUser.HasRole(UserRoles.SearchContacts),
                new NewContactsWidget() { WidgetType = DashboardWidgetType.NewContacts, Title = AppLocalization["Contacts created in the last 14 days"] });

            AddWidgetIf(isSuperAdmin
                || applicationUser.HasRole(UserRoles.SearchUsers)
                || applicationUser.HasRole(UserRoles.SearchOUs)
                || applicationUser.HasRole(UserRoles.SearchGroups)
                || applicationUser.HasRole(UserRoles.SearchPrinters)
                || applicationUser.HasRole(UserRoles.SearchComputers),
                new ChangedEntriesWidget() { WidgetType = DashboardWidgetType.ChangedEntries, Title = AppLocalization["Entries changed in the last 24 hours"] });

            AddWidgetIf(isSuperAdmin || applicationUser.HasRole(UserRoles.SearchOUs),
                new NewOUsWidget() { WidgetType = DashboardWidgetType.NewOus, Title = AppLocalization["OU's created in the last 14 days"] });

            AddWidgetIf(isSuperAdmin || applicationUser.HasRole(UserRoles.SearchGroups),
                new NewGroupsWidget() { WidgetType = DashboardWidgetType.NewGroups, Title = AppLocalization["Groups created in the last 14 days"] });

            AddWidgetIf(isSuperAdmin || applicationUser.HasRole(UserRoles.SearchPrinters),
                new NewPrintersWidget() { WidgetType = DashboardWidgetType.NewPrinters, Title = AppLocalization["Printers created in the last 14 days"] });

            AddWidgetIf(isSuperAdmin || applicationUser.HasRole(UserRoles.SearchComputers),
                new NewComputersWidget() { WidgetType = DashboardWidgetType.NewComputers, Title = AppLocalization["Computers created in the last 14 days"] });

            if (isSuperAdmin)
            {
                widgets.Add(new DeletedEntriesWidget() { WidgetType = DashboardWidgetType.DeletedEntries, Title = AppLocalization["Entries deleted in the last 14 days"] });
                widgets.Add(new AppLogonsWidget() { WidgetType = DashboardWidgetType.AppLogons, Title = AppLocalization[Lang.Application_logons] });
                widgets.Add(new StaleUsersWidget() { WidgetType = DashboardWidgetType.StaleUsers, Title = AppLocalization[Lang.Stale_users] });
                widgets.Add(new StaleComputersWidget() { WidgetType = DashboardWidgetType.StaleComputers, Title = AppLocalization[Lang.Stale_computers] });
            }

            widgets.Add(new FavoritesWidget() { WidgetType = DashboardWidgetType.FavoriteEntries, Title = AppLocalization[Lang.Favorites] });

            return widgets;
        }

    }
}
