using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.ActiveDirectory.Services;
using BLAZAM.Database.Context;
using BLAZAM.Helpers;
using BLAZAM.Jobs;
using BLAZAM.Localization;
using BLAZAM.Logger;
using Microsoft.Extensions.Localization;

namespace BLAZAM.Services.Background
{
    /// <summary>
    /// Prefills the user table with all users who have login access
    /// </summary>
    [AutoStartBackgroundService]
    public class UserSeederService : ActiveDirectoryBackgroundServiceBase
    {
        private readonly ApplicationInfo _applicationInfo;

        public UserSeederService(ApplicationInfo applicationInfo, IActiveDirectoryContextFactory activeDirectoryContextFactory, IAppDatabaseFactory dbFactory, IStringLocalizer<AppLocalization> appLocalization) : base(activeDirectoryContextFactory, dbFactory, appLocalization)
        {
            Interval = TimeSpan.FromMinutes(60);

            _applicationInfo = applicationInfo;
        }

        protected override void Execute(object? obj = null)
        {
            Job seedJob = new(AppLocalization["Seed New Users"]);
            JobStep step = new(AppLocalization["Check for new users"], (state) =>
            {
                try
                {
                    EnsureAdminExists();
                    EnsureSelfExists();

                    if (_applicationInfo.InDemoMode)
                        EnsureDemoExists();
                    using var context = dbFactory.CreateDbContext();
                    using var activeDirectoryContext = activeDirectoryContextFactory.CreateActiveDirectoryContext();
                    if (context.Status != ServiceConnectionState.Up) return false;
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
                    return false;
                }
                return true;
            });
            seedJob.AddStep(step);
            var result = seedJob.Run();

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
