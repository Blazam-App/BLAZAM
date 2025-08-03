using BLAZAM.Database.Context;
using BLAZAM.Database.Services;
using BLAZAM.Helpers;
using BLAZAM.Jobs;
using BLAZAM.Localization;
using BLAZAM.Logger;
using BLAZAM.Update;
using BLAZAM.Update.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace BLAZAM.Services.Background
{
    [AutoStartBackgroundService]
    public class AutoUpdateService : DatabaseBackgroundServiceBase, IDisposable
    {

        public AppDelegate<DateTime?> OnAutoUpdateQueued { get; set; }
        public AppDelegate OnAutoUpdateStarted { get; set; }
        public AppDelegate OnAutoUpdateFailed { get; set; }

        private readonly ApplicationInfo _applicationInfo;

        private readonly IAppDatabaseFactory factory;
        private UpdateService updateService;
        private Timer? autoUpdateApplyTimer = null;
        private Timer? directoryCleaner = null;
        public bool IsUpdatedScheduled { get { return autoUpdateApplyTimer != null; } }
        public DateTime ScheduledUpdateTime { get; set; }
        public ApplicationUpdate? ScheduledUpdate { get; set; }
        public bool IsUpdateAvailable { get; private set; }

        //private AuditLogger Audit;

        public AutoUpdateService(IAppDatabaseFactory factory, UpdateService updateService, ApplicationInfo applicationInfo, IStringLocalizer<AppLocalization> appLocalization) : base(factory,appLocalization)
        {
            Interval = TimeSpan.FromMinutes(60);

            _applicationInfo = applicationInfo;
            this.factory = factory;
            this.updateService = updateService;
            directoryCleaner = new Timer(CleanDirectories, null, TimeSpan.FromSeconds(30), TimeSpan.FromHours(20));
        }

        private void CleanDirectories(object? state)
        {
            using var context = factory.CreateDbContext();

            var oldUpdateFiles = ApplicationUpdate.UpdateDownloadDirectory.Files;
            foreach (var file in oldUpdateFiles)
            {
                try
                {
                    var fileVersion = new ApplicationVersion(file.Name);
                    if (fileVersion.OlderThan(_applicationInfo.RunningVersion) && file.SinceLastModified > TimeSpan.FromDays(1))
                    {
                        if (file.Writable)
                        {
                            Loggers.UpdateLogger.Debug("Deleting old update file: " + file);

                            file.Delete();

                        }
                        else
                        {
                            Loggers.UpdateLogger.Warning("Attempting Update credentials to delete old update file: " + file);

                            var impersonation = context.AppSettings.FirstOrDefault()?.CreateUpdateImpersonator();
                            if (impersonation != null && !impersonation.Run(() =>
                            {
                                if (file.Writable)
                                {
                                    file.Delete();
                                    return true;
                                }
                                return false;
                            }))
                            {
                                impersonation = context.ActiveDirectorySettings.FirstOrDefault()?.CreateDirectoryAdminImpersonator();
                                if (impersonation != null && !impersonation.Run(() =>
                                {
                                    if (file.Writable)
                                    {
                                        file.Delete();
                                        return true;
                                    }
                                    return false;
                                }))
                                {
                                    Loggers.UpdateLogger.Error("No identities with permission to remove old update file: " + file);

                                }
                            }
                        }
                    }
                }
                catch (IndexOutOfRangeException ex)
                {
                    Loggers.UpdateLogger.Warning("Tried to delete non-existent file: " + file, ex);
                }
                catch (Exception ex)
                {
                    Loggers.UpdateLogger.Error( ex, "Other error deleting file. {@File} ",file);
                }

            }


            var oldStaginDirectories = ApplicationUpdate.StagingDirectory.SubDirectories;
            foreach (var dir in oldStaginDirectories)
            {
                try
                {


                    var dirVersion = new ApplicationVersion(dir.Name);
                    if (dirVersion != null)
                    {
                        if (dirVersion.OlderThan(_applicationInfo.RunningVersion))
                        {
                            if (dir.Writable)
                            {

                                Loggers.UpdateLogger.Debug("Deleting old staged update directory: {@Directory}", dir.ToString());
                                dir.Delete(true);
                            }
                            else
                            {
                                Loggers.UpdateLogger.Warning("Attempting Update credentials to delete old staging files");

                                var impersonation = context.AppSettings.FirstOrDefault()?.CreateUpdateImpersonator();
                                if (impersonation != null && !impersonation.Run(() =>
                                {
                                    if (dir.Writable)
                                    {

                                        Loggers.UpdateLogger.Debug("Deleting old staged update directory: {@Directory}", dir.ToString());
                                        dir.Delete(true);
                                        return true;
                                    }
                                    return false;
                                }))
                                {
                                    impersonation = context.ActiveDirectorySettings.FirstOrDefault()?.CreateDirectoryAdminImpersonator();
                                    if (impersonation != null && !impersonation.Run(() =>
                                    {
                                        if (dir.Writable)
                                        {

                                            Loggers.UpdateLogger.Debug("Deleting old staged update directory: {@Directory}", dir.ToString());
                                            dir.Delete(true);
                                            return true;

                                        }
                                        return false;
                                    }))
                                    {
                                        Loggers.UpdateLogger.Error("No identities with permission to remove old staging files");

                                    }
                                }
                            }
                        }


                    }
                }
                catch (IndexOutOfRangeException ex)
                {
                    Loggers.UpdateLogger.Error(ex, "Deleting unknown directory: {@Directory}", dir.ToString());
                    //dir.Delete(true);
                }
                catch (Exception ex)
                {
                    Loggers.UpdateLogger.Error(ex,"Other error cleaning staging files {@Directory}", dir.FullPath);
                    //file.Delete();
                }
            }

        }

        protected override void Execute(object? state)
        {
            using var context = factory.CreateDbContext();
            Job updateCheckJob = new(AppLocalization[Lang.Check_for_Update]);
            JobStep checkForUpdateStep = new(AppLocalization[Lang.Excute], async (step) =>
            {
                try
                {
                    var appSettings = await context.AppSettings.FirstOrDefaultAsync();
                    if (appSettings != null)
                    {
                        Loggers.UpdateLogger.Information("Checking for automatic update");

                        var latestUpdate = await updateService.GetUpdates();
                        if (latestUpdate != null && latestUpdate.Version.NewerThan(_applicationInfo.RunningVersion))
                        {
                            IsUpdateAvailable = true;
                            if (appSettings.AutoUpdate && appSettings.AutoUpdateTime != null)
                            {
                                if (latestUpdate.PassesPrerequisiteChecks)
                                    ScheduleUpdate(appSettings.AutoUpdateTime.Value, latestUpdate);
                                else
                                {
                                    Loggers.UpdateLogger.Warning("Update failed prerequisite check, cancelling scheduling {@Error}", latestUpdate.PrequisiteMessage);

                                }
                            }

                        }
                        else
                        {
                            IsUpdateAvailable = false;
                            Cancel();
                            Loggers.UpdateLogger.Information("No new updates found.");

                        }
                    }
                }
                catch (Exception ex)
                {
                    Loggers.UpdateLogger.Error(ex, "Error while checking for auto update");
                }
                return true;
            });
            updateCheckJob.AddStep(checkForUpdateStep);
            updateCheckJob.Run();
        }
        public void Cancel()
        {
            ScheduledUpdate = null;
            ScheduledUpdateTime = DateTime.MinValue;
            autoUpdateApplyTimer?.Dispose();
            autoUpdateApplyTimer = null;
        }

        public void ScheduleUpdate(TimeSpan updateTimeOfDay, ApplicationUpdate updateToInstall)
        {
            try
            {
                bool justScheduled = ScheduledUpdateTime == DateTime.MinValue && ScheduledUpdate != updateToInstall;
                if (ScheduledUpdate != updateToInstall)
                {
                    Job scheduleUpdatteJob = new("Schedule Update");
                    JobStep scheduleStep = new("Execute", (step) =>
                    {
                        try
                        {

                            Loggers.UpdateLogger.Information("New update found: " + updateToInstall.Version);

                            //Update available
                            var now = DateTime.Now;
                            ScheduledUpdateTime = new DateTime(now.Year, now.Month, now.Day, updateTimeOfDay.Hours, updateTimeOfDay.Minutes, updateTimeOfDay.Seconds);


                            //Check if we're past the scheduled time this day
                            if (ScheduledUpdateTime < now)
                            {
                                ScheduledUpdateTime = ScheduledUpdateTime.AddDays(1);
                            }


                            TimeSpan timeUntilUpdate = ScheduledUpdateTime - now;

                            ScheduledUpdate = updateToInstall;

                            autoUpdateApplyTimer = new Timer(Update, null, (int)timeUntilUpdate.TotalMilliseconds, Timeout.Infinite);
                            Loggers.UpdateLogger.Information("Auto-update scheduled: " + timeUntilUpdate.TotalMinutes + "mins from now at " + ScheduledUpdateTime);
                            if (justScheduled)
                            {
                                Loggers.UpdateLogger.Debug("Update just scheduled");
                                OnAutoUpdateQueued?.Invoke(ScheduledUpdateTime);

                            }


                        }
                        catch (Exception ex)
                        {
                            Loggers.UpdateLogger.Error(ex,  "Error during auto update scheduling");
                        }
                        return true;
                    });
                    scheduleUpdatteJob.AddStep(scheduleStep);
                    scheduleUpdatteJob.Run();
                }
            }
            catch (Exception ex)
            {
                Loggers.UpdateLogger.Error(ex, "Error during auto update scheduling");
            }
        }

        private async void Update(object? state)
        {
            Loggers.UpdateLogger.Information("Attempting auto-update");
            try
            {
                using var context = await factory.CreateDbContextAsync();
                var settings = await context.AppSettings.FirstOrDefaultAsync();
                if (settings != null)
                {
                    if (settings.AutoUpdate)
                    {
                        Loggers.UpdateLogger.Information("Applying auto-update");
                        Loggers.UpdateLogger.Information("Current Version: {@Version}", _applicationInfo.RunningVersion);
                        Loggers.UpdateLogger.Information("Update Version: {@UpdateVersion}", ScheduledUpdate?.Version);

                        autoUpdateApplyTimer = null;
                        ScheduledUpdateTime = DateTime.MinValue;
                        var latestUpdate = await updateService.GetUpdates();
                        if (latestUpdate != null)
                        {
                            try
                            {
                                OnAutoUpdateStarted?.Invoke();
                                var updateJob = latestUpdate.GetUpdateJob();
                                if (updateJob != null)
                                {
                                    await updateJob.RunAsync();
                                    if (updateJob.Result == JobResult.Passed)
                                        Loggers.UpdateLogger.Information("Auto-update applied. Application will now reboot.{@UpdateVersion}", latestUpdate.Version);
                                    else
                                    {
                                        var thrownStep = updateJob.Steps.FirstOrDefault(x => x.Exception != null);
                                        if (thrownStep != null)
                                            Loggers.UpdateLogger.Error(thrownStep.Exception,"Failed to auto-update. {@UpdateVersion}", latestUpdate.Version);
                                        else
                                            Loggers.UpdateLogger.Error("Failed to auto-update. No exception collected from update job!{@UpdateVersion}", latestUpdate.Version);

                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                //Log.Error(ex);
                                OnAutoUpdateFailed?.Invoke();
                                Loggers.UpdateLogger.Error(ex, "Error trying to apply auto update {@UpdateVersion}", latestUpdate.Version);
                            }
                        }
                        else
                        {
                            Loggers.UpdateLogger.Error("Unable to get latest update {@Branch}{@AutoUpdate}{@AutoUpdateTime}", settings.UpdateBranch, settings.AutoUpdate, settings.AutoUpdateTime);

                        }
                    }
                    else
                    {
                        //Auto Update was turned off since scheduling
                        Loggers.UpdateLogger.Warning("Auto Update was turned off after scheduling");

                    }
                }
                else
                {
                    Loggers.UpdateLogger.Error("Unable to get update database settings");
                }
            }
            catch (Exception ex)
            {
                Loggers.UpdateLogger.Error(ex, "Error during auto update");
            }


        }

        public void Dispose()
        {
            autoUpdateApplyTimer?.Dispose();
            directoryCleaner?.Dispose();
        }
    }
}
