using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.ActiveDirectory.Services;
using BLAZAM.Global.Enums;
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

        protected override void Execute(object? state = null)
        {
            Job seedJob = new(AppLocalization["Seed New Users"]);
            seedJob.StopOnFailedStep = true;

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

                    var delegates = context.PermissionDelegate
                        .Where(x => x.DeletedAt == null)
                        .ToList();

                    foreach (var deleg in delegates)
                    {
                        var entry = activeDirectoryContext.FindEntryBySID(deleg.DelegateSid);
                        ProcessDirectoryEntry(entry);
                    }
                }
                catch (Exception ex)
                {
                    Loggers.SystemLogger.Error(ex, "Error attempting to synchronize directory and application users.");
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
                    Username = user.SAMAccountName,
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
        /// <summary>
        /// Processes an Active Directory entry, ensuring users are seeded.
        /// </summary>
        private async Task ProcessDirectoryEntry(object? entry)
        {
            if (entry is IADUser user)
            {
                EnsureUserExists(user);
            }
            else if (entry is IADGroup group)
            {
                foreach (var member in await group.GetNestedMembersAsync())
                {
                    if (member is IADUser aduser)
                        EnsureUserExists(aduser);
                }
            }
        }
    }
}
