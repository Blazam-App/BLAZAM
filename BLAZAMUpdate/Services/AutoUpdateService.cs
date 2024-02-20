using BLAZAM.Common;
using BLAZAM.Common.Data;
using BLAZAM.Database.Context;
using BLAZAM.Helpers;
using BLAZAM.Jobs;
using BLAZAM.Logger;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using System.DirectoryServices.Protocols;

namespace BLAZAM.Update.Services
{
    public class AutoUpdateService
    {

        public AppEvent<DateTime?> OnAutoUpdateQueued { get; set; }
        public AppEvent OnAutoUpdateStarted { get; set; }
        public AppEvent OnAutoUpdateFailed { get; set; }

        private readonly ApplicationInfo _applicationInfo;

        private readonly IAppDatabaseFactory factory;
        private UpdateService updateService;
        private Timer? updateCheckTimer;
        private Timer? autoUpdateApplyTimer = null;
        private Timer? directoryCleaner = null;
        public bool IsUpdatedScheduled { get { return autoUpdateApplyTimer != null; } }
        public DateTime ScheduledUpdateTime { get; set; }
        public ApplicationUpdate? ScheduledUpdate { get; set; }
        //private AuditLogger Audit;

        public AutoUpdateService(IAppDatabaseFactory factory, UpdateService updateService, ApplicationInfo applicationInfo)
        {
            _applicationInfo = applicationInfo;
            this.factory = factory;
            this.updateService = updateService;
            updateCheckTimer = new Timer(CheckForUpdate, null, (int)TimeSpan.FromSeconds(20).TotalMilliseconds, (int)TimeSpan.FromHours(1).TotalMilliseconds);
            directoryCleaner = new Timer(CleanDirectories, null, (int)TimeSpan.FromSeconds(30).TotalMilliseconds, (int)TimeSpan.FromHours(20).TotalMilliseconds);
        }

        private void CleanDirectories(object? state)
        {
            var oldUpdateFiles = ApplicationUpdate.UpdateDownloadDirectory.Files;
            foreach (var file in oldUpdateFiles)
            {
                try
                {
                    var fileVersion = new ApplicationVersion(file.Name);
                    if (fileVersion.CompareTo(_applicationInfo.RunningVersion) < 0 && file.SinceLastModified > TimeSpan.FromDays(1))
                    {
                        if (file.Writable)
                        {
                            Loggers.UpdateLogger.Debug("Deleting old update file: " + file);

                            file.Delete();

                        }
                        else
                        {
                            Loggers.UpdateLogger.Warning("Attempting Update credentials to delete old update file: " + file);

                            var impersonation = factory.CreateDbContext().AppSettings.FirstOrDefault()?.CreateUpdateImpersonator();
                            if (impersonation!=null && !impersonation.Run(() =>
                            {
                                if (file.Writable)
                                {
                                    file.Delete();
                                    return true;
                                }
                                return false;
                            }))
                            {
                                impersonation = factory.CreateDbContext().ActiveDirectorySettings.FirstOrDefault()?.CreateDirectoryAdminImpersonator();
                                if (impersonation !=null && !impersonation.Run(() =>
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
                    //file.Delete();
                }
                catch (Exception ex)
                {
                    Loggers.UpdateLogger.Error("Other error deleting file: " + file + " {@Error}", ex);
                    //file.Delete();
                }

            }
            //TODO Fix the staging cleanup


            var oldStaginDirectories = ApplicationUpdate.StagingDirectory.SubDirectories;
            foreach (var dir in oldStaginDirectories)
            {
                try
                {


                    var dirVersion = new ApplicationVersion(dir.Name);
                    if (dirVersion != null)
                    {
                        if (dirVersion.CompareTo(_applicationInfo.RunningVersion) < 0)
                        {
                            if (dir.Writable)
                            {

                                Loggers.UpdateLogger.Debug("Deleting old staged update directory: " + dir);
                                dir.Delete(true);
                            }
                            else
                            {
                                Loggers.UpdateLogger.Warning("Attempting Update credentials to delete old staging files");

                                var impersonation = factory.CreateDbContext().AppSettings.FirstOrDefault()?.CreateUpdateImpersonator();
                                if (impersonation != null && !impersonation.Run(() =>
                                {
                                    if (dir.Writable)
                                    {

                                        Loggers.UpdateLogger.Debug("Deleting old staged update directory: " + dir);
                                        dir.Delete(true);
                                        return true;
                                    }
                                    return false;
                                }))
                                {
                                    impersonation = factory.CreateDbContext().ActiveDirectorySettings.FirstOrDefault()?.CreateDirectoryAdminImpersonator();
                                    if (impersonation != null && !impersonation.Run(() =>
                                    {
                                        if (dir.Writable)
                                        {

                                            Loggers.UpdateLogger.Debug("Deleting old staged update directory: " + dir);
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
                    Loggers.UpdateLogger.Error("Deleting unknown directory: " + dir+ "{@Error}",ex);
                    //dir.Delete(true);
                }
                catch (Exception ex)
                {
                    Loggers.UpdateLogger.Error("Other error cleaning staging files {Directory}{@Error}", dir.Path, ex);
                    //file.Delete();
                }
            }

        }

        private async void CheckForUpdate(object? state)
        {
            try
            {
                var appSettings = (await factory.CreateDbContextAsync()).AppSettings.FirstOrDefault();

                Loggers.UpdateLogger.Information("Checking for automatic update");

                var latestUpdate = await updateService.GetUpdates();
                if (latestUpdate != null && latestUpdate.Version.CompareTo(_applicationInfo.RunningVersion) > 0 && appSettings.AutoUpdate && appSettings.AutoUpdateTime != null)
                {
                    ScheduleUpdate(appSettings.AutoUpdateTime.Value, latestUpdate);
                }
                else
                {
                    Loggers.UpdateLogger.Information("No new updates found.");

                }

            }
            catch (Exception ex)
            {
                Loggers.UpdateLogger.Error("Error while checking for auto update {@Error}", ex);
            }
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
                    Loggers.UpdateLogger.Information("New update found: " + updateToInstall.Version);

                    //Update availabled
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
            }
            catch (Exception ex)
            {
                Loggers.UpdateLogger.Error("Error during auto update scheduling {@Error}", ex);
            }
        }

        private async void Update(object? state)
        {
            Loggers.UpdateLogger.Information("Attempting auto-update");
            try
            {
                using var context = await factory.CreateDbContextAsync();
                var settings = context.AppSettings.FirstOrDefault();
                if (settings != null)
                {
                    if (settings.AutoUpdate)
                    {
                        Loggers.UpdateLogger.Information("Applying auto-update");
                        Loggers.UpdateLogger.Information("Current Version: " + _applicationInfo.RunningVersion);
                        Loggers.UpdateLogger.Information("Update Version: " + ScheduledUpdate?.Version);

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
                                    updateJob.Run();
                                    if (updateJob.Result == JobResult.Passed)
                                        Loggers.UpdateLogger.Information("Auto-update applied. Application will now reboot.{@UpdateVersion}", latestUpdate.Version);
                                    else
                                    {
                                        var thrownStep = updateJob.Steps.FirstOrDefault(x => x.Exception != null);
                                        if (thrownStep != null)
                                            Loggers.UpdateLogger.Error("Failed to auto-update. {@Error}{@UpdateVersion}", thrownStep.Exception, latestUpdate.Version);
                                        else
                                            Loggers.UpdateLogger.Error("Failed to auto-update. No exception collected from update job!{@UpdateVersion}", latestUpdate.Version);

                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                //Log.Error(ex);
                                OnAutoUpdateFailed?.Invoke();
                                Loggers.UpdateLogger.Error("Error trying to apply auto update {@Error}{@UpdateVersion}", ex, latestUpdate.Version);
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
                        //Audit.System.LogMessage("Auto Update was turned off after scheduling");
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
                Loggers.UpdateLogger.Error("Error during auto update {@Error}", ex);
            }


        }
    }
}
