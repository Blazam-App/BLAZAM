using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.ActiveDirectory.Services;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models;
using BLAZAM.Database.Models.Notifications;
using BLAZAM.Helpers;
using BLAZAM.Jobs;
using BLAZAM.Localization;
using BLAZAM.Logger;
using BLAZAM.Services.Audit;
using BLAZAM.Session;
using Microsoft.Extensions.Localization;
using System.Security.Cryptography.Xml;

namespace BLAZAM.Services.Background
{
    /// <summary>
    /// Checks the directory for expired users, if configured, those users will be disabled
    /// </summary>
    //TODO Re-enable this service when a setting is added to db or the rules take its place and remove this
    //[AutoStartBackgroundService]
    internal class ExpiredUserDisabler : ActiveDirectoryBackgroundServiceBase
    {
        private readonly NotificationGenerationService _notificationGenerationService;
        private readonly ServerAuditLogger _serverAuditLogger;

        public ExpiredUserDisabler(ServerAuditLogger serverAuditLogger, NotificationGenerationService notificationGenerationService, IActiveDirectoryContextFactory activeDirectoryContextFactory, IAppDatabaseFactory dbFactory, IStringLocalizer<AppLocalization> appLocalization) : base(activeDirectoryContextFactory, dbFactory,appLocalization)
        {
            Interval = TimeSpan.FromMinutes(30);
            _notificationGenerationService = notificationGenerationService;
            _serverAuditLogger = serverAuditLogger;
        }

        protected override void Execute(object? state = null)
        {
            using var context = dbFactory.CreateDbContext();
            using var directory = activeDirectoryContextFactory.CreateActiveDirectoryContext();

            var expiredUsers = new List<IADUser>(); 
            Job executeJob = new(AppLocalization["Disable Expired Users"]);
            executeJob.StopOnFailedStep = true;

            JobStep prepareStep = new(AppLocalization["Collect data"], (state) =>
            {
                expiredUsers = directory.Users.FindExpiredUsers().Where(u=>u.ExpireTime!=null && u.ExpireTime<DateTime.UtcNow).ToList();
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
                        List<AuditChangeLog>? changes = new(user.Changes);
                        var result = user.CommitChanges(); 
                        
                        if(result.Result == JobResult.Passed)
                        {
                            _serverAuditLogger.User.Changed(user,changes);
                            _notificationGenerationService.PostAsync(user, NotificationType.Modify,new SystemUserState(dbFactory));
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
