using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.ActiveDirectory.Services;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models;
using BLAZAM.Database.Models.Notifications;
using BLAZAM.Helpers;
using BLAZAM.Jobs;
using BLAZAM.Logger;
using BLAZAM.Services.Audit;
using System.Security.Cryptography.Xml;

namespace BLAZAM.Services.Background
{
    [AutoStartBackgroundService(30)]
    internal class ExpiredUserDisabler : ActiveDirectoryBackgroundServiceBase
    {
        private NotificationGenerationService _notificationGenerationService;

        public ExpiredUserDisabler(NotificationGenerationService notificationGenerationService, IActiveDirectoryContextFactory activeDirectoryContextFactory, IAppDatabaseFactory dbFactory) : base(activeDirectoryContextFactory, dbFactory)
        {
            _notificationGenerationService = notificationGenerationService;
        }

        protected override void Execute(object? state = null)
        {
            using var context = dbFactory.CreateDbContext();
            using var directory = activeDirectoryContextFactory.CreateActiveDirectoryContext();

            var expiredUsers = new List<IADUser>(); 
            Job executeJob = new("Disable Expired Users");
            JobStep prepareStep = new("Collect data", (state) =>
            {
                expiredUsers = directory.Users.FindExpiredUsers().Where(u=>u.ExpireTime!=null && u.ExpireTime<DateTime.UtcNow).ToList();
                return true;
            });
            executeJob.AddStep(prepareStep);
            JobStep analyzeStep = new("Disable Users", (state) =>
            {
                foreach (var user in expiredUsers)
                {
                    if (user == null) continue;
                    if (user.Enabled)
                    {
                        var original = directory.Users.FindUserBySID(user.SID.ToSidString());
                        user.Enabled = false;
                        var changes = user.GetChanges(original);
                        var result = user.CommitChanges(); 
                        
                        if(result.Result == JobResult.Passed)
                        {
                            _notificationGenerationService.PostAsync(user, NotificationType.Modify);
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
