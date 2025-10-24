using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.ActiveDirectory.Services;
using BLAZAM.Helpers;
using BLAZAM.Jobs;
using BLAZAM.Localization;
using BLAZAM.Services.Events;
using BLAZAM.Session;
using Microsoft.Extensions.Localization;

namespace BLAZAM.Services.Background
{
    /// <summary>
    /// Checks the directory for expired users, if configured, those users will be disabled
    /// </summary>
    //TODO Re-enable this service when a setting is added to db or the rules take its place and remove this
    //[AutoStartBackgroundService]
    internal class ExpiredUserDisabler : ActiveDirectoryBackgroundServiceBase
    {

        public ExpiredUserDisabler(IActiveDirectoryContextFactory activeDirectoryContextFactory, IAppDatabaseFactory dbFactory, IStringLocalizer<AppLocalization> appLocalization) : base(activeDirectoryContextFactory, dbFactory, appLocalization)
        {
            Interval = TimeSpan.FromMinutes(30);
        }

        protected override void Execute(object? state = null)
        {
            using var context = dbFactory.CreateDbContext();
            using var directory = activeDirectoryContextFactory.CreateActiveDirectoryContext();

            var expiredUsers = new List<IADUser>();
            Job executeJob = new(AppLocalization["Disable Expired Users"])
            {
                StopOnFailedStep = true
            };

            JobStep prepareStep = new(AppLocalization["Collect data"], (state) =>
            {
                expiredUsers = directory.Users.FindExpiredUsers().Where(u => u.ExpireTime != null && u.ExpireTime < DateTime.UtcNow).ToList();
                return true;
            });
            executeJob.AddStep(prepareStep);
            JobStep analyzeStep = new(AppLocalization["Disable Users"], (state) =>
            {
                foreach (var user in expiredUsers)
                {
                    if (user == null) continue;
                    if (user.Enabled)
                    {
                        var original = directory.Users.FindUserBySID(user.SID.ToSidString());
                        user.Enabled = false;
                        List<AuditChangeLog>? changes = [.. user.Changes];
                        var result = user.CommitChanges();

                        if (result.Result == JobResult.Passed)
                        {
                            ApplicationEvents.DirectoryEntryEvent.Invoke(new()
                            {
                                EventType = ApplicationEventType.Modify,
                                Entry = user,
                                Changes = changes,
                                Actor = new SystemUserState(dbFactory)

                            });
                        }

                    }
                }

                return true;
            });
            executeJob.AddStep(analyzeStep);
            var result = executeJob.Run();


        }
    }
}
