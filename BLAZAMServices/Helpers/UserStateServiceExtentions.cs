using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Services;
using BLAZAM.Services.Background;
using BLAZAM.Session;
using BLAZAM.Session.Interfaces;

namespace BLAZAM.Helpers
{
    public static class UserStateServiceExtentions
    {
        public static async Task<IApplicationUserState?> GetApplicationUser(this IDirectoryEntryAdapter entry, IApplicationUserStateService userStateService,
            IAppDatabaseFactory appDatabaseFactory,
            IActiveDirectoryContext directory,
            AppAuthenticationStateProvider appAuthenticationStateProvider)
        {
            var permissionApplicator = new PermissionApplicator(userStateService, appDatabaseFactory, directory);
            var appUser = new ApplicationUserState(appDatabaseFactory);
            await permissionApplicator.LoadPermissions(appUser, entry as IADUser);
            appUser.User = await appAuthenticationStateProvider.CreateDirectoryPrincipal(appUser,entry as IADUser);
            appUser.GetUserSettingFromDB();
            return appUser;
        }
    }
}
