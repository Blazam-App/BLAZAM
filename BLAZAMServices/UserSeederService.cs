using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Database.Context;
using BLAZAM.Helpers;
using BLAZAM.Logger;
using BLAZAM.Services.Background;

namespace BLAZAM.Services
{
    /// <summary>
    /// Prefills the user table with all users who have login access
    /// </summary>
    [AutoStartBackgroundService(60)]
    public class UserSeederService:ActiveDirectoryBackgroundServiceBase
    {
        private readonly ApplicationInfo _applicationInfo;

        public UserSeederService(ApplicationInfo applicationInfo,IActiveDirectoryContextFactory activeDirectoryContextFactory, IAppDatabaseFactory dbFactory) : base(activeDirectoryContextFactory, dbFactory)
        {
            _applicationInfo= applicationInfo;
        }

        protected override void Execute(object? obj = null)
        {
            try
            {
                EnsureAdminExists();
                EnsureSelfExists();

                if (_applicationInfo.InDemoMode)
                    EnsureDemoExists();
                using var context = dbFactory.CreateDbContext();
                using var activeDirectoryContext = activeDirectoryContextFactory.CreateActiveDirectoryContext();
                if (context.Status != ServiceConnectionState.Up) return;
                foreach (var deleg in context.PermissionDelegate.Where(x => x.DeletedAt == null).ToList())
                {
                    var entry = activeDirectoryContext.FindEntryBySID(deleg.DelegateSid);
                    if (entry != null)
                    {
                        if (entry is IADUser user)
                        {
                            EnsureUserExists(user);
                        }
                        if (entry is IADGroup group)
                        {
                            foreach (var member in group.NestedMembers)
                            {
                                var type = member.GetType();
                                if (member is IADUser aduser)
                                    EnsureUserExists(aduser);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error("Error attempting to synchronize directory and application users. {@Error}", ex);
            }
        }
        /// <summary>
        /// Checks the database for this user, if not found they are added
        /// </summary>
        /// <param name="user"></param>
        private void EnsureUserExists(IADUser user)
        {
            using var context = dbFactory.CreateDbContext();
            if (!context.UserSettings.Any(us => us.UserGUID == user.SID.ToSidString()))
            {
                context.UserSettings.Add(new()
                {
                    Username = user.SamAccountName,
                    UserGUID = user.SID.ToSidString(),
                    Email = user.Email
                });
            }
            context.SaveChanges();
        }
        /// <summary>
        /// Checks the database for the admin user, if not found it is added
        /// </summary>
        private void EnsureAdminExists()
        {
            using var context = dbFactory.CreateDbContext();
            if (!context.UserSettings.Any(us => us.UserGUID == "1"))
            {
                context.UserSettings.Add(new()
                {
                    Username = "admin",
                    UserGUID = "1"
                });
            }
            context.SaveChanges();
        }
        /// <summary>
        /// Checks the database for the demo user, if not found it is added
        /// </summary>
        private void EnsureDemoExists()
        {
            using var context = dbFactory.CreateDbContext();
            if (!context.UserSettings.Any(us => us.UserGUID == "2"))
            {
                context.UserSettings.Add(new()
                {
                    Username = "Demo",
                    UserGUID = "2"
                });
            }
            context.SaveChanges();
        }
        /// <summary>
        /// Checks the database for the self user, if not found it is added
        /// </summary>
        private void EnsureSelfExists()
        {
            using var context = dbFactory.CreateDbContext();
            if (!context.UserSettings.Any(us => us.UserGUID == "3"))
            {
                context.UserSettings.Add(new()
                {
                    Username = "Self",
                    UserGUID = "3"
                });
            }
            context.SaveChanges();
        }
    }
}
