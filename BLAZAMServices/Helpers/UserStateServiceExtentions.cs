using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Context;
using BLAZAM.Services;
using BLAZAM.Services.Background;
using BLAZAM.Session;
using BLAZAM.Session.Interfaces;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
