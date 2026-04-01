using BLAZAM.Common.Data;
using BLAZAM.FileSystem;
using BLAZAM.Helpers;
using BLAZAM.Jobs;
using BLAZAM.Logger;
using BLAZAM.Update.Exceptions;
using BLAZAM.Update.Services;
using System.Diagnostics;
using System.IO.Compression;
using System.Security.Principal;
using System.Text;

namespace BLAZAM.Update
{
    public enum UpdateStage { None, Downloading, Downloaded, Staging, Staged, BackingUp, Prepared, Applying, Applied };

    public class ApplicationUpdate : IEquatable<ApplicationUpdate?>
    {



        public UpdateStage UpdateStage { get; set; }
        /// <summary>
        /// Token source for cancelling this update when in progress
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource { get; set; } = new CancellationTokenSource();

        public static AppDelegate OnUpdateStarted { get; set; }

        public static AppDelegate<Exception> OnUpdateFailed { get; set; }

        /// <summary>
        /// The version of this update
        /// </summary>
        public ApplicationVersion Version { get => Release.Version; set => Release.Version = value; }

        public string Branch { get => Release.Branch; }

        private readonly ApplicationInfo _applicationInfo;
        private readonly IAppDatabaseFactory _dbFactory;
        private readonly UpdateService _updateService;

       /// <summary>
       /// Use <see cref="UpdateTempDirectory"/> instead
       /// </summary>
        private SystemDirectory? _updateTempDirectory;
        /// <summary>
        /// The application update directory, in temporary files
        /// </summary>
        /// <returns>
        /// eg: C:\user\appdata\local\temp\BLAZAM\update\
        ///</returns>

        private SystemDirectory UpdateTempDirectory
        {
            get
            {
                if (_updateTempDirectory == null)
                {
                    _updateTempDirectory = _updateService.GetUpdateIdentityTempDirectory();
                }
                return _updateTempDirectory;
            }
        }

        public SystemDirectory StagingDirectory =>
            new(_applicationRootDirectory.FullPath + Path.DirectorySeparatorChar + "updater" + Path.DirectorySeparatorChar + "staged" + Path.DirectorySeparatorChar);

        /// <summary>
        /// The local staging directory path for this update
        /// </summary>
        public SystemDirectory UpdateStagingDirectory { get => StagingDirectory; }



        /// <summary>
        /// The local path to the directory containing the downloaded update zip file
        /// </summary>
        /// <returns>
        /// eg: C:\inetpub\blazam\Writable\Update\Download\
        /// </returns>
        public SystemDirectory UpdateDownloadDirectory
        {
            get => new(UpdateTempDirectory + "download" + Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// The local path to the downloaded zip file
        /// </summary>
        public SystemFile UpdateFile { get => new(UpdateDownloadDirectory + Version.Version + ".zip"); }
        public string UpdateCommand => UpdateCommandProcess;
        public static string UpdateCommandProcess
        {
            get
            {
                return "Powershell";
            }
        }
      
        private SystemFile CommandProcessPath
        {
            get
            {
                var testPath = new SystemFile(_applicationRootDirectory + $"bin{Path.DirectorySeparatorChar}Debug{Path.DirectorySeparatorChar}net8.0{Path.DirectorySeparatorChar}updater{Path.DirectorySeparatorChar}update.ps1");


                if (!testPath.Exists)
                {
                    testPath = new SystemFile(_applicationRootDirectory + $"updater{Path.DirectorySeparatorChar}update.ps1");
                }
                return testPath;
            }
        }
       

        /// <summary>
        /// Called when download progress has changed
        /// </summary>
        public AppDelegate<FileProgress?> DownloadPercentageChanged { get; set; }

        private readonly ApplicationVersion _runningVersion;
        private readonly SystemDirectory _applicationRootDirectory;
        private WindowsImpersonation _updateIdentity = new(null);

        public ApplicationUpdate(ApplicationInfo applicationInfo, UpdateService updateService, IAppDatabaseFactory dbFactory)
        {
            _applicationInfo = applicationInfo;
            _dbFactory = dbFactory;
            _updateService = updateService;
            _runningVersion = applicationInfo.RunningVersion;
            _applicationRootDirectory = applicationInfo.ApplicationRoot;
        }

        /// <summary>
        /// True if this version is newer than the running version
        /// </summary>
        public bool Newer
        {
            get { return Version.NewerThan(_runningVersion); }
        }

        public IApplicationRelease Release { get; set; }

        public List<Func<bool>> PreRequisiteChecks { get; private set; } = [];

        public bool PassesPrerequisiteChecks
        {
            get
            {
                if (PreRequisiteChecks.Count == 0)
                {
                    return true;
                }

                foreach (var check in PreRequisiteChecks)
                {
                    if (!check.Invoke())
                    {
                        Loggers.UpdateLogger?.Warning("Update prerequisite check failed");
                        return false;
                    }
                }
                return true;
            }
        }

        public string PrequisiteMessage { get; internal set; }

        public IJob GetUpdateJob()
        {
            Loggers.UpdateLogger?.Information("Creating update job for version {Version}", Version);

            if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource = new CancellationTokenSource();
            }

            Job updateJob = new("Applying application update", "System", _cancellationTokenSource)
            {
                StopOnFailedStep = true
            };
            var ensureProfileStep = new JobStep("Ensure update credential profile exists", EnsureProfileExists);
            var cleanDownloadStep = new JobStep("Cleaning previous downloads", CleanDownload);
            var downloadStep = new JobStep("Download latest version", Download);
            var cleanStageStep = new JobStep("Cleaning staging area", CleanStaging);
            var stageStep = new JobStep("Extract files", ExtractFiles);
            var stagingCheckStep = new JobStep("Check prepared files", CheckExtractedFiles);
            var bakupStep = new JobStep("Create backup", Backup);
            var updateUpdaterStep = new JobStep("Update updater", UpdateUpdater);
            var updateStep = new JobStep("Apply Files", InitiateFileCopy);
            var waitForRestart = new JobStep("Wait for completion...", Wait);
            updateJob.AddStep(ensureProfileStep);
            updateJob.AddStep(cleanDownloadStep);
            updateJob.AddStep(downloadStep);
            updateJob.AddStep(cleanStageStep);
            updateJob.AddStep(stageStep);
            updateJob.AddStep(stagingCheckStep);
            updateJob.AddStep(bakupStep);
            updateJob.AddStep(updateUpdaterStep);
            //updateJob.AddStep(updateStep);
            //updateJob.AddStep(waitForRestart);

            Loggers.UpdateLogger?.Debug("Update job created with {StepCount} steps", updateJob.Steps.Count);
            return updateJob;



        }

        private bool EnsureProfileExists(JobStep? step)
        {
            Loggers.UpdateLogger?.Debug("Ensuring update identity profile exists");
            try
            {
                _ = UpdateTempDirectory;

                _updateIdentity = _updateService.GetUpdateIdentity();
                // Ensure profile exists before impersonation
                _updateIdentity.EnsureProfileExists();
                Loggers.UpdateLogger?.Information("Update identity profile verified for user: {Username}", _updateIdentity.ImpersonationUser?.Username ?? "Application");
                return true;
            }
            catch (Exception ex)
            {
                Loggers.UpdateLogger?.Error(ex, "Failed to ensure update identity profile exists");
                return false;
            }
        }
        private bool CheckExtractedFiles(JobStep? step)
        {
            Loggers.UpdateLogger?.Debug("Checking extracted files in staging directory: {StagingDirectory}", UpdateStagingDirectory.FullPath);
            return _updateIdentity.Run(() =>
            {
                bool exists = UpdateStagingDirectory.Exists;
                int fileCount = exists ? UpdateStagingDirectory.Files.Count : 0;
                bool passed = exists && fileCount > 3;

                if (passed)
                {
                    Loggers.UpdateLogger?.Information("Extracted files verified. Found {FileCount} files in staging directory", fileCount);
                }
                else
                {
                    Loggers.UpdateLogger?.Warning("Extracted files check failed. Directory exists: {Exists}, File count: {FileCount}", exists, fileCount);
                }

                return passed;
            });
        }

        private async Task<bool> Backup(JobStep? step)
        {
            Loggers.UpdateLogger?.Information("Starting application backup before applying update");
            var progress = new Progress<FileProgress>(p =>
            {
                step!.Progress = p.FilePercentage;
            });
            bool result = await _updateService.Backup(progress);

            if (result)
            {
                Loggers.UpdateLogger?.Information("Application backup completed successfully");
            }
            else
            {
                Loggers.UpdateLogger?.Error("Application backup failed");
            }

            return result;
        }

        private bool UpdateUpdater(JobStep step)
        {
            return _updateIdentity.Run(() =>
            {
                var updaterSource = new SystemDirectory(Path.Combine(UpdateStagingDirectory.FullPath,
                                                                                "updater"));
                var updaterDestination = new SystemDirectory(Path.Combine(_applicationRootDirectory.FullPath,
                                                                                    "updater"));
                Loggers.UpdateLogger.Debug("Copying updater script");
                Loggers.UpdateLogger.Debug("Source: {Source}", updaterSource);
                Loggers.UpdateLogger.Debug("Dest: {Destination}", updaterDestination);
                if (updaterSource.Exists && updaterSource.Files.Count > 0 && updaterSource.CopyTo(updaterDestination))
                {
                    Loggers.UpdateLogger.Debug("Updater script copied successfully");
                    return true;
                }
                else
                {
                    Loggers.UpdateLogger.Error("Failed to copy updater script from {Source} to {Destination}", updaterSource, updaterDestination);
                }
                return false;
            });
        }

        private static async Task<bool> Wait(JobStep? step)
        {
            Loggers.UpdateLogger?.Information("Waiting for update process to complete (60 second timeout)");
            await Task.Delay(60000);
            Loggers.UpdateLogger?.Warning("Update wait period expired, application should have restarted");
            return false;
        }

        private async Task<bool> InitiateFileCopy(JobStep? step)
        {
            var cmd = UpdateCommandProcess;
            var stopwatch = new Stopwatch();
            stopwatch.Start();


            Loggers.UpdateLogger.Information("Initiating file copy with command: {Command} {Arguments}", cmd);
            Loggers.UpdateLogger.Debug("Running as user: {Username}", _updateIdentity.ImpersonationUser?.Username ?? "Application");

            var proc = Process.Start("schtasks /run /tn \"Update Blazam\"");
            

            Loggers.UpdateLogger.Information("File copy command executed with result: {Success} in {ElapsedMilliseconds}ms", proc.ExitCode == 0, stopwatch.ElapsedMilliseconds);

            stopwatch.Stop();

            return proc.ExitCode == 0;

        }



        public async Task<bool> CleanDownload(IJobStep? step)
        {
            return await _updateIdentity.RunAsync(() =>
            {
                Loggers.UpdateLogger?.Information("Attempting cleaning of download folder: {@UpdatePath}", UpdateFile.FullPath);

                try
                {
                    UpdateDownloadDirectory.ClearDirectory();
                    Loggers.UpdateLogger?.Debug("Download folder cleaned successfully");
                    return true;

                }
                catch (Exception ex)
                {
                    Loggers.UpdateLogger?.Error(ex, "Error while cleaning of download folder: {@UpdatePath}", UpdateFile.FullPath);

                    return false;
                }
            });

        }
        public async Task<bool> CleanStaging(IJobStep? step)
        {
            Loggers.UpdateLogger?.Information("Attempting to clean staging directory: {StagingDirectory}", UpdateStagingDirectory.FullPath);
            return await _updateIdentity.RunAsync(() =>
            {
                try
                {
                    UpdateStagingDirectory.Delete(true);
                    Loggers.UpdateLogger?.Debug("Staging directory cleaned successfully");
                    return true;

                }
                catch (Exception ex)
                {
                    Loggers.UpdateLogger?.Error(ex, "Error while cleaning staging directory: {StagingDirectory}", UpdateStagingDirectory.FullPath);
                    return true;
                }
            });
        }
        public async Task<bool> ExtractFiles(JobStep? step)
        {
            return await _updateIdentity.RunAsync(() =>
            {

                if (!UpdateFile.Exists)
                {
                    Loggers.UpdateLogger?.Error("Update file does not exist: {UpdateFile}", UpdateFile.FullPath);
                    return false;
                }

                Loggers.UpdateLogger?.Debug("Attempting unzip of {UpdatePath}", UpdateFile.FullPath);

                UpdateStagingDirectory.EnsureCreated();

                using var streamToReadFrom = UpdateFile.OpenReadStream();
                if (streamToReadFrom == null)
                {
                    Loggers.UpdateLogger?.Error("Failed to open read stream for update file: {UpdateFile}", UpdateFile.FullPath);
                    return false;
                }
                try
                {
                    var zip = new ZipArchive(streamToReadFrom);
                    zip.ExtractToDirectory(UpdateStagingDirectory.FullPath, true);
                    Loggers.UpdateLogger?.Debug("{UpdatePath} unzipped successfully to {StagingPath}", UpdateFile.FullPath, UpdateStagingDirectory.FullPath);

                    return true;
                }
                catch (Exception ex)
                {
                    Loggers.UpdateLogger?.Error(ex, "Error while extracting update zip");

                    return false;

                }
            });

        }

        public void Cancel()
        {
            Loggers.UpdateLogger?.Warning("Update process cancellation requested");
            _cancellationTokenSource?.Cancel();
            Loggers.UpdateLogger?.Information("Update process cancelled");
        }
        public async Task<bool> Download(JobStep? step)
        {

            return await _updateIdentity.RunAsync(async () =>
            {
                if (Release == null)
                {
                    Loggers.UpdateLogger?.Error("Cannot download update: Release is null");
                    return false;
                }

                int retries = 5;
                int currentAttempt = 1;
                var progress = new FileProgress();

                while (retries > 0)
                {
                    try
                    {
                        if (currentAttempt > 1)
                        {
                            Loggers.UpdateLogger?.Warning("Retrying download, attempt {Attempt} of 5", currentAttempt);
                        }
                        LogDownloadAttempt();

                        using (var client = new HttpClient())
                        using (var response = await client.GetAsync(Release.DownloadURL, HttpCompletionOption.ResponseHeadersRead))
                        {
                            if (!response.IsSuccessStatusCode)
                            {
                                Loggers.UpdateLogger?.Error("Unable to connect to download url: {StatusCode}:{ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                                return false;
                            }

                            UpdateDownloadDirectory.EnsureCreated();
                            if (UpdateFile.Exists)
                            {
                                Loggers.UpdateLogger?.Debug("Deleting existing update file");
                                UpdateFile.Delete();
                            }

                            using var streamToReadFrom = await response.Content.ReadAsStreamAsync();
                            using var streamToWriteTo = UpdateFile.OpenWriteStream();
                            progress.ExpectedSize = (int)Release.ExpectedSize.GetValueOrDefault();
                            bool result = await WriteStreamWithProgress(streamToReadFrom, streamToWriteTo, progress, step);
                            if (!result)
                            {
                                Loggers.UpdateLogger?.Warning("Download was cancelled or failed");
                                return false;
                            }
                            Loggers.UpdateLogger?.Information("Download completed successfully. File size: {FileSize} bytes", progress.CompletedBytes);
                        }
                        retries = 0;
                    }
                    catch (Exception ex)
                    {
                        retries--;
                        currentAttempt++;
                        if (retries == 0)
                        {
                            Loggers.UpdateLogger?.Error(ex, "Failed to download update after all retry attempts");
                            throw new ApplicationUpdateException("Failed to download update", ex);
                        }
                        else
                        {
                            Loggers.UpdateLogger?.Warning(ex, "Download failed, {RemainingRetries} retries remaining. Waiting 3 seconds before retry", retries);
                            await Task.Delay(3000);
                        }
                    }
                }
                return true;
            });
        }

        private void LogDownloadAttempt()
        {
            Loggers.UpdateLogger?.Debug("Attempting download of update {@UpdateVersion}", Version);
            Loggers.UpdateLogger?.Debug("Download URL: {@DownloadURL}", Release.DownloadURL);
            Loggers.UpdateLogger?.Debug("Download Path: {@UpdateDirectory}", UpdateDownloadDirectory.FullPath);
        }

        private async Task<bool> WriteStreamWithProgress(Stream readStream, Stream writeStream, FileProgress progress, JobStep? step)
        {
            var buffer = new byte[4096];
            int bytesRead;
            int totalBytesRead = 0;

            while ((bytesRead = await readStream.ReadAsync(buffer, 0, buffer.Length, _cancellationTokenSource.Token)) > 0)
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    Loggers.UpdateLogger?.Warning("Download cancelled by user. Downloaded {BytesDownloaded} bytes before cancellation", totalBytesRead);
                    DownloadPercentageChanged?.Invoke(null);
                    return false;
                }

                await writeStream.WriteAsync(buffer, 0, bytesRead, _cancellationTokenSource.Token);
                totalBytesRead += bytesRead;
                progress.CompletedBytes = totalBytesRead;
                if (step != null)
                {
                    step.Progress = progress.FilePercentage;
                }
                DownloadPercentageChanged?.Invoke(progress);
            }
            Loggers.UpdateLogger?.Debug("Stream write completed. Total bytes written: {TotalBytes}", totalBytesRead);
            return true;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as ApplicationUpdate);
        }

        public bool Equals(ApplicationUpdate? other)
        {
            return other is not null &&
                   EqualityComparer<ApplicationVersion>.Default.Equals(Version, other.Version);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Version);
        }

        public static bool operator ==(ApplicationUpdate? left, ApplicationUpdate? right)
        {
            return EqualityComparer<ApplicationUpdate>.Default.Equals(left, right);
        }

        public static bool operator !=(ApplicationUpdate? left, ApplicationUpdate? right)
        {
            return !(left == right);
        }
    }
}
