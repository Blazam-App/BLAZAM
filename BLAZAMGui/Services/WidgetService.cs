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
                new LockedOutUsers());

            AddWidgetIf(isSuperAdmin || applicationUser.HasRole(UserRoles.SearchUsers),
                new NewUsersWidget());

            AddWidgetIf(isSuperAdmin || applicationUser.HasRole(UserRoles.SearchUsers),
                new ChangedPasswordsWidget());

            AddWidgetIf(isSuperAdmin || applicationUser.HasRole(UserRoles.SearchUsers),
                new DisabledUsersWidget());

            AddWidgetIf(isSuperAdmin || applicationUser.HasRole(UserRoles.SearchContacts),
                new NewContactsWidget());

            AddWidgetIf(isSuperAdmin
                || applicationUser.HasRole(UserRoles.SearchUsers)
                || applicationUser.HasRole(UserRoles.SearchOUs)
                || applicationUser.HasRole(UserRoles.SearchGroups)
                || applicationUser.HasRole(UserRoles.SearchPrinters)
                || applicationUser.HasRole(UserRoles.SearchComputers),
                new ChangedEntriesWidget());

            AddWidgetIf(isSuperAdmin || applicationUser.HasRole(UserRoles.SearchOUs),
                new NewOUsWidget());

            AddWidgetIf(isSuperAdmin || applicationUser.HasRole(UserRoles.SearchGroups),
                new NewGroupsWidget());

            AddWidgetIf(isSuperAdmin || applicationUser.HasRole(UserRoles.SearchPrinters),
                new NewPrintersWidget());

            AddWidgetIf(isSuperAdmin || applicationUser.HasRole(UserRoles.SearchComputers),
                new NewComputersWidget());

            if (isSuperAdmin)
            {
                widgets.Add(new DeletedEntriesWidget());
                widgets.Add(new AppLogonsWidget());
                widgets.Add(new StaleUsersWidget());
                widgets.Add(new StaleComputersWidget());
            }

            widgets.Add(new FavoritesWidget());

            return widgets;
        }

    }
}
