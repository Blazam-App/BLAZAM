using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.ActiveDirectory.Services;
using BLAZAM.Database.Models;
using BLAZAM.Helpers;
using BLAZAM.Jobs;
using BLAZAM.Localization;
using BLAZAM.Services.Events;
using BLAZAM.Session;
using Microsoft.Extensions.Localization;

namespace BLAZAM.Services.Background
{
    [AutoStartBackgroundService]
    public class LockedOutUserMonitor : ActiveDirectoryBackgroundServiceBase
    {


        public LockedOutUserMonitor(IActiveDirectoryContextFactory activeDirectoryContextFactory, IAppDatabaseFactory dbFactory, IStringLocalizer<AppLocalization> appLocalization) : base(activeDirectoryContextFactory, dbFactory, appLocalization)
        {
            Interval = TimeSpan.FromMinutes(10);

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
                return AnalyzeUsers(directory, usersInTable, lockedOutUsers);
            });
            executeJob.AddStep(analyzeStep);
            var result = executeJob.Run();


        }

        private bool AnalyzeUsers(IActiveDirectoryContext directory, List<GenericSidList> usersInTable, List<IADUser> lockedOutUsers)
        {
            using var context = dbFactory.CreateDbContext();

            AddNewlyLockedOutUsers(context, usersInTable, lockedOutUsers);
            RemoveUnlockedUsers(context, directory, usersInTable);

            context.SaveChanges();
            return true;
        }

        private void AddNewlyLockedOutUsers(IDatabaseContext context, List<GenericSidList> usersInTable, List<IADUser> lockedOutUsers)
        {
            foreach (var user in lockedOutUsers.Where(u => u != null && u.LockedOut))
            {
                string sid = user.SID.ToSidString();
                if (!usersInTable.Any(x => x.Sid == sid))
                {
                    context.LockedOutUsers.Add(new() { Sid = sid, Added = DateTime.UtcNow });

                    if (user.LockoutTime > DateTime.UtcNow.AddDays(-1))
                    {
                        ApplicationEvents.DirectoryEntryEvent.Invoke(new()
                        {
                            EventType = ApplicationEventType.LockedOut,
                            Entry = user,
                            Actor = new SystemUserState(dbFactory)
                        });

                        if (OperatingSystem.IsWindows())
                        {
                            RecordLogonEvents(user);
                        }
                    }
                }
            }
        }

        private void RemoveUnlockedUsers(IDatabaseContext context, IActiveDirectoryContext directory, List<GenericSidList> usersInTable)
        {
            foreach (var user in usersInTable.Where(u => u != null))
            {
                var adUser = directory.FindEntryBySid(user.Sid) as IADUser;
                if (adUser != null && !adUser.LockedOut)
                {
                    var existing = context.LockedOutUsers.FirstOrDefault(x => x.Sid == user.Sid);
                    if (existing != null)
                    {
                        context.LockedOutUsers.Remove(existing);
                    }
                }
            }
        }

        public void RecordLogonEvents(IADUser user)
        {
            using var context = dbFactory.CreateDbContext();
            var existing = context.FailedADLogonEvents.Where(e => e.Sid.Equals(user.SID)).OrderBy(e => e.Timestamp).ToList();

            var failedLogonEvents = user.FailedLogonEvents.OrderBy(e => e.Timestamp).ToList();
            if (failedLogonEvents.Count > 0)
            {

                foreach (var evt in failedLogonEvents.Where(e => existing == null || existing.Count == 0 || e.Timestamp > existing.LastOrDefault()?.Timestamp))
                //foreach (var evt in failedLogonEvents)
                {
                    var matching = context.FailedADLogonEvents.FirstOrDefault(e => e.Timestamp.Equals(evt.Timestamp));
                    if (matching == null)
                    {

                        if (existing.Count > 9)
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
