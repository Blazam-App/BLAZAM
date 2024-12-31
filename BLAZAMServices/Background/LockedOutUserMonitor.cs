using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models;
using BLAZAM.Database.Models.Notifications;
using BLAZAM.Helpers;
using BLAZAM.Jobs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Services.Background
{
    [AutoStartBackgroundService(10)]
    internal class LockedOutUserMonitor : ActiveDirectoryBackgroundServiceBase
    {
        private NotificationGenerationService _notificationGenerationService;

        public LockedOutUserMonitor(NotificationGenerationService notificationGenerationService, IActiveDirectoryContextFactory activeDirectoryContextFactory, IAppDatabaseFactory dbFactory) : base(activeDirectoryContextFactory, dbFactory)
        {
            _notificationGenerationService = notificationGenerationService;
        }

        protected override void Execute(object? state = null)
        {
            using var context = dbFactory.CreateDbContext();
            using var directory = activeDirectoryContextFactory.CreateActiveDirectoryContext();

            List<GenericSidList> usersInTable = new List<GenericSidList>();

            List<IADUser> lockedOutUsers = new List<IADUser>();
            Job executeJob = new Job("Monitor Locked Out Users");
            JobStep prepareStep = new JobStep("Prepare data", (state) =>
            {
                usersInTable = context.LockedOutUsers.ToList();
                lockedOutUsers = directory.Users.FindLockedOutUsers();
                return true;
            });
            executeJob.AddStep(prepareStep);
            JobStep analyzeStep = new JobStep("Analyze data", (state) =>
            {
                foreach (var user in lockedOutUsers)
                {
                    if (user == null) continue;
                    if (user.LockedOut)
                    {
                        if (!usersInTable.Any(x => x.Sid == user.SID.ToSidString()))
                        {
                            //add user to lockout table
                            context.LockedOutUsers.Add(new() { Sid = user.SID.ToSidString(), Added = DateTime.UtcNow });

                            if(user.LockoutTime > DateTime.UtcNow.AddDays(-1))
                            {
                                _notificationGenerationService.PostAsync(user, NotificationType.LockedOut);
                            }
                        }

                    }
                }
                foreach (var user in usersInTable)
                {
                    if (user == null) continue;
                    var adUser = directory.GetDirectoryEntryBySid(user.Sid) as IADUser;
                    if (adUser!=null && !adUser.LockedOut)
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
    }
}
