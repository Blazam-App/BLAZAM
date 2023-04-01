using BLAZAM.Common;
using BLAZAM.Common.Data.Database;
using BLAZAM.Common.Models.Database;
using BLAZAM.Server.Data.Services.Email;
using BLAZAM.Server.Shared.Email;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace BLAZAM.Server.Data.Services.Update
{
    public class AutoUpdateService
    {

        public AppEvent<DateTime?> OnAutoUpdateQueued { get; set; }
        public AppEvent OnAutoUpdateStarted { get; set; }
        public AppEvent OnAutoUpdateFailed { get; set; }

        private readonly EmailService email;
        private readonly AppDatabaseFactory factory;
        private UpdateService updateService;
        private Timer? updateCheckTimer;
        private Timer? autoUpdateApplyTimer = null;
        private Timer? directoryCleaner = null;
        public bool IsUpdatedScheduled { get { return autoUpdateApplyTimer != null; } }
        public DateTime ScheduledUpdateTime { get; set; }
        public ApplicationUpdate? ScheduledUpdate { get; set; }
        //private AuditLogger Audit;

        public AutoUpdateService(AppDatabaseFactory factory, UpdateService updateService, EmailService emailService)
        {
            this.email = emailService;
            this.factory = factory;
            //Audit = auditLogger;
            this.updateService = updateService;
            updateCheckTimer = new Timer(CheckForUpdate, null, (int)TimeSpan.FromHours(0).TotalMilliseconds, (int)TimeSpan.FromHours(1).TotalMilliseconds);
            directoryCleaner = new Timer(CleanDirectories, null, (int)TimeSpan.FromSeconds(30).TotalMilliseconds, (int)TimeSpan.FromDays(1).TotalMilliseconds);
        }

        private void CleanDirectories(object? state)
        {
            var oldUpdateFiles = ApplicationUpdate.UpdateDownloadDirectory.Files;
            foreach (var file in oldUpdateFiles)
            {
                try
                {
                    var fileVersion = new ApplicationVersion(file.Name);
                    if (fileVersion.CompareTo(Program.Version) < 0 && file.SinceLastModified > TimeSpan.FromDays(1))
                    {
                        Loggers.UpdateLogger.Debug("Deleting old update file: " + file);
                        file.Delete();
                    }
                }
                catch (IndexOutOfRangeException ex)
                {
                    Loggers.UpdateLogger.Debug("Deleting unknown file: " + file,ex);
                    file.Delete();
                }

            }
            /*
             * Disable to test strange behavior, the staging folders are deleted on shutdown
             * during an update
            var oldStaginDirectories = ApplicationUpdate.StagingDirectory.SubDirectories;
            foreach (var dir in oldStaginDirectories)
            {
                try
                {
                    var dirVersion = new ApplicationVersion(dir.Name);
                    if (dirVersion.CompareTo(Program.Version) < 0)
                    {
                        Loggers.UpdateLogger.Debug("Deleting old staged update directory: " + dir);
                        dir.Delete(true);
                    }
                }
                catch (IndexOutOfRangeException ex)
                {
                    Loggers.UpdateLogger.Debug("Deleting unknown directory: " + dir);
                    dir.Delete(true);
                }
              
            }
            */
        }

        private async void CheckForUpdate(object? state)
        {
            try
            {
                var appSettings = (await factory.CreateDbContextAsync()).AppSettings.FirstOrDefault();
                if (appSettings.AutoUpdate)
                {
                    Loggers.UpdateLogger.Information("Checking for automatic update");

                    var latestUpdate = await updateService.GetLatestUpdate();
                    if (latestUpdate.Version.CompareTo(Program.Version) > 0 && appSettings.AutoUpdateTime!=null)
                    {
                        ScheduleUpdate(appSettings.AutoUpdateTime.Value, latestUpdate);
                    }
                    else
                    {
                        Loggers.UpdateLogger.Information("No new updates found.");

                    }
                }
            }
            catch (Exception ex)
            {
                Loggers.UpdateLogger.Error("Error while checking for auto update", ex);
            }
        }
        public void Cancel()
        {
            ScheduledUpdate = null;
            ScheduledUpdateTime = DateTime.MinValue;
            autoUpdateApplyTimer.Dispose();
            autoUpdateApplyTimer = null;
        }

        public void ScheduleUpdate(TimeSpan updateTimeOfDay, ApplicationUpdate updateToInstall)
        {

            bool justScheduled = ScheduledUpdateTime == DateTime.MinValue && ScheduledUpdate != updateToInstall ;
            if (ScheduledUpdate != updateToInstall)
            {
                Loggers.UpdateLogger.Information("New update found: " + updateToInstall.Version);

                //Update availabled
                var now = DateTime.Now;
                ScheduledUpdateTime = new DateTime(now.Year, now.Month, now.Day, updateTimeOfDay.Hours, updateTimeOfDay.Minutes, updateTimeOfDay.Seconds);


                //Check if we're past the scheduled time this day
                if (ScheduledUpdateTime < now)
                {
                    ScheduledUpdateTime = ScheduledUpdateTime.AddDays(1);
                }


                TimeSpan timeUntilUpdate = (ScheduledUpdateTime - now);

                ScheduledUpdate = updateToInstall;

                autoUpdateApplyTimer = new Timer(Update, null, (int)timeUntilUpdate.TotalMilliseconds, Timeout.Infinite);
                Loggers.UpdateLogger.Information("Auto-update scheduled: " + timeUntilUpdate.TotalMinutes + "mins from now at " + ScheduledUpdateTime);
                if (justScheduled)
                {
                    Loggers.UpdateLogger.Debug("Update just scheduled, so sending notification email to admins");

                    try
                    {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        email.SendMessage(
                            "Update Schedueled",
                            "admin@blazam.org",
                            (MarkupString)"Update Scheduled",
                            (MarkupString)("The application has schedueled an update to version "
                            + ScheduledUpdate.Version + " at " + ScheduledUpdateTime
                            ));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }
                    catch (Exception ex)
                    {
                        Loggers.UpdateLogger.Error("Error while sending auto update scheduled email", ex);

                    }
                }

                OnAutoUpdateQueued?.Invoke(ScheduledUpdateTime);
            }
        }

        private async void Update(object? state)
        {
            Loggers.UpdateLogger.Information("Attempting auto-update");
            try
            {
                using var context = await factory.CreateDbContextAsync();
                var settings = context.AppSettings.FirstOrDefault();
                if (settings.AutoUpdate)
                {
                    Loggers.UpdateLogger.Information("Applying auto-update");
                    Loggers.UpdateLogger.Information("Current Version: " + Program.Version);
                    Loggers.UpdateLogger.Information("Update Version: " + ScheduledUpdate.Version);

                    autoUpdateApplyTimer = null;
                    ScheduledUpdateTime = DateTime.MinValue;
                    var latestUpdate = await updateService.GetLatestUpdate();
                    try
                    {
                        OnAutoUpdateStarted?.Invoke();
                        var result = await latestUpdate.Apply();
                        if (result != null)
                        {
                            Loggers.UpdateLogger.Information("Auto-update applied. Application will now reboot. Response: "+result);

                        }
                    }
                    catch (Exception ex)
                    {
                        //Log.Error(ex);
                        OnAutoUpdateFailed?.Invoke();
                        Loggers.UpdateLogger.Error("Error trying to apply auto update", ex);
                    }
                }
                else
                {
                    //Auto Update was turned off since scheduling
                    //Audit.System.LogMessage("Auto Update was turned off after scheduling");
                    Loggers.UpdateLogger.Debug("Auto Update was turned off after scheduling");

                }
            }
            catch (Exception ex)
            {
                Loggers.UpdateLogger.Error("Execptions attempting auto-update", ex);

            }
        }
    }
}
