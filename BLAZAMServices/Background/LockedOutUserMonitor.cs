using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.ActiveDirectory.Services;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models;
using BLAZAM.Database.Models.Notifications;
using BLAZAM.Helpers;
using BLAZAM.Jobs;
using BLAZAM.Localization;
using Microsoft.Extensions.Localization;
using Polly;

namespace BLAZAM.Services.Background
{
    [AutoStartBackgroundService]
    public class LockedOutUserMonitor : ActiveDirectoryBackgroundServiceBase
    {
        private NotificationGenerationService _notificationGenerationService;

        public LockedOutUserMonitor(NotificationGenerationService notificationGenerationService, IActiveDirectoryContextFactory activeDirectoryContextFactory, IAppDatabaseFactory dbFactory, IStringLocalizer<AppLocalization> appLocalization) : base(activeDirectoryContextFactory, dbFactory, appLocalization)
        {
            Interval = TimeSpan.FromMinutes(10);

            _notificationGenerationService = notificationGenerationService;
        }

        protected override void Execute(object? state = null)
        {
            using var directory = activeDirectoryContextFactory.CreateActiveDirectoryContext();

            List<GenericSidList> usersInTable = new();

            List<IADUser> lockedOutUsers = new();
            Job executeJob = new(AppLocalization["Monitor Locked Out Users"]);

            executeJob.StopOnFailedStep = true;

            JobStep prepareStep = new(AppLocalization["Prepare data"], (state) =>
            {
                using var context = dbFactory.CreateDbContext();
                usersInTable = context.LockedOutUsers.ToList();
                lockedOutUsers = directory.Users.FindLockedOutUsers();
                return true;
            });
            executeJob.AddStep(prepareStep);
            JobStep analyzeStep = new(AppLocalization["Analyze data"], (state) =>
            {
                using var context = dbFactory.CreateDbContext();
                foreach (var user in lockedOutUsers)
                {
                    if (user == null) continue;
                    if (user.LockedOut)
                    {
                        if (!usersInTable.Any(x => x.Sid == user.SID.ToSidString()))
                        {
                            //add user to lockout table
                            context.LockedOutUsers.Add(new() { Sid = user.SID.ToSidString(), Added = DateTime.UtcNow });

                            if (user.LockoutTime > DateTime.UtcNow.AddDays(-1))
                            {
                                _notificationGenerationService.PostAsync(user, NotificationType.LockedOut);

                                RecordLogonEvents(user);

                            }
                        }

                    }
                }
                foreach (var user in usersInTable)
                {
                    if (user == null) continue;
                    var adUser = directory.GetDirectoryEntryBySid(user.Sid) as IADUser;
                    if (adUser != null && !adUser.LockedOut)
                    {
                        var existing = context.LockedOutUsers.FirstOrDefault(x => x.Sid == user.Sid);
                        if (existing != null)
                        {
                            context.LockedOutUsers.Remove(existing);

                        }



                    }
                }
                context.SaveChanges();
                return true;
            });
            executeJob.AddStep(analyzeStep);
            var result = executeJob.Run();


        }

        public void RecordLogonEvents(IADUser user)
        {
            using var context = dbFactory.CreateDbContext();
            var existing = context.FailedADLogonEvents.Where(e => e.Sid.Equals(user.SID)).OrderBy(e => e.Timestamp).ToList();

            var failedLogonEvents = user.FailedLogonEvents.OrderBy(e=>e.Timestamp).ToList();
            if (failedLogonEvents.Count > 0)
            {

                foreach (var evt in failedLogonEvents.Where(e=> existing == null || existing.Count == 0 || e.Timestamp>existing.LastOrDefault()?.Timestamp))
                //foreach (var evt in failedLogonEvents)
                {
                    var matching = context.FailedADLogonEvents.FirstOrDefault(e => e.Timestamp.Equals(evt.Timestamp));
                    if (matching == null)
                    {

                        if (existing.Count() > 9)
                        {
                            context.FailedADLogonEvents.Remove(existing.First());
                            existing.Remove(existing.First());
                        }
                        context.FailedADLogonEvents.Add(evt);
                        existing.Add(evt);

                    }

                }
            }
            context.SaveChanges();
        }
    }
}
