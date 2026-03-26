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
            new(UpdateTempDirectory + "staged" + Path.DirectorySeparatorChar);

        /// <summary>
        /// The local staging directory path for this update
        /// </summary>
        public SystemDirectory UpdateStagingDirectory { get => new(StagingDirectory + Version.Version); }



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
        public string UpdateCommand => UpdateCommandProcess + " " + UpdateCommandArguments;
        public static string UpdateCommandProcess
        {
            get
            {
                return "Powershell";
            }
        }
        public string UpdateCommandArguments
        {
            get
            {
                return "-ExecutionPolicy Bypass -command \"& '" + CommandProcessPath
                    + "' " + CommandArguments + "\"";
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
        private string CommandArguments
        {
            get
            {

                var args = " -UpdateSourcePath '" + UpdateStagingDirectory + "' -ProcessId " + _runningProcess.Id + " -ApplicationDirectory '" + _applicationRootDirectory;



                if (Debugger.IsAttached)
                {
                    args += $"bin{Path.DirectorySeparatorChar}Debug{Path.DirectorySeparatorChar}net8.0{Path.DirectorySeparatorChar}";
                }

                args += "'";

                //switch (_updateService.UpdateCredential)
                //{
                //    case UpdateCredential.Active_Directory:
                //        var adSettings = _dbFactory.CreateDbContext().ActiveDirectorySettings.FirstOrDefault();
                //        if (adSettings != null)
                //        {
                //            args += $" -Username '{adSettings.Username}' -Password '{adSettings.Password.Decrypt()}' -Domain '{adSettings.FQDN}'";
                //        }
                //        break;
                //    case UpdateCredential.Custom:
                //        var appSettings = _dbFactory.CreateDbContext().AppSettings.FirstOrDefault();
                //        if (appSettings != null)
                //        {
                //            args += $" -Username '{appSettings.UpdateUsername}' -Password '{appSettings.UpdatePassword?.Decrypt()}' -Domain '{appSettings.UpdateDomain}'";
                //        }
                //        break;

                //}


                return args;

            }
        }

        /// <summary>
        /// Called when download progress has changed
        /// </summary>
        public AppDelegate<FileProgress?> DownloadPercentageChanged { get; set; }

        private readonly ApplicationVersion _runningVersion;
        private readonly Process _runningProcess;
        private readonly SystemDirectory _applicationRootDirectory;
        private WindowsImpersonation _updateIdentity;

        public ApplicationUpdate(ApplicationInfo applicationInfo, UpdateService updateService, IAppDatabaseFactory dbFactory)
        {
            _applicationInfo = applicationInfo;
            _dbFactory = dbFactory;
            _updateService = updateService;
            _runningProcess = applicationInfo.RunningProcess;
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
                        return false;
                    }
                }
                return true;
            }
        }

        public string PrequisiteMessage { get; internal set; }

        public IJob GetUpdateJob()
        {

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
            updateJob.AddStep(updateStep);
            updateJob.AddStep(waitForRestart);
            return updateJob;



        }

        private bool EnsureProfileExists(JobStep? step)
        {
            _updateIdentity = _updateService.GetUpdateIdentity();
            // Ensure profile exists before impersonation
            _updateIdentity.EnsureProfileExists();
            return true;
        }
        private bool CheckExtractedFiles(JobStep? step)
        {
            return _updateIdentity.Run(() =>
            {
                return UpdateStagingDirectory.Exists && UpdateStagingDirectory.Files.Count > 3;
            });
        }
        
        private async Task<bool> Backup(JobStep? step)
        {
            var progress = new Progress<FileProgress>(p =>
            {
                step!.Progress = p.FilePercentage;
            });
            return await _updateService.Backup(progress); 
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

            await Task.Delay(60000);
            return false;
        }

        private async Task<bool> InitiateFileCopy(JobStep? step)
        {
            var cmd = UpdateCommandProcess;
            var args = UpdateCommandArguments;
            var stopwatch = new Stopwatch();
            stopwatch.Start();


            Loggers.UpdateLogger.Information("Initiating file copy with command: {Command} {Arguments}", cmd, args);
            var runAs = new RunAs(_updateIdentity.ImpersonationUser);

            // Execute a command

            bool success = runAs.ExecuteCommand(cmd + " " + args);

            Loggers.UpdateLogger.Information("File copy command executed with result: {Success} in {ElapsedMilliseconds}ms", success, stopwatch.ElapsedMilliseconds);

            stopwatch.Stop();

            return success;

        }



        public async Task<bool> CleanDownload(IJobStep? step)
        {
            return await _updateIdentity.RunAsync(() =>
            {
                Loggers.UpdateLogger?.Information("Attempting cleaning of download folder: {@UpdatePath}", UpdateFile);

                try
                {
                    UpdateDownloadDirectory.ClearDirectory();

                    return true;

                }
                catch (Exception ex)
                {
                    Loggers.UpdateLogger?.Error(ex, "Error while cleaning of download folder: {@UpdatePath}", UpdateFile);

                    return false;
                }
            });

        }
        public async Task<bool> CleanStaging(IJobStep? step)
        {
            return await _updateIdentity.RunAsync(() =>
            {
                try
                {
                    UpdateStagingDirectory.Delete(true);
                    return true;

                }
                catch (Exception ex)
                {
                    Loggers.UpdateLogger?.Error(ex, "Error while cleaning staging directory.");
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
                    return false;
                }

                Loggers.UpdateLogger?.Debug("Attempting unzip of {UpdatePath}", UpdateFile);

                UpdateStagingDirectory.EnsureCreated();

                using var streamToReadFrom = UpdateFile.OpenReadStream();
                if (streamToReadFrom == null)
                {
                    return false;
                }
                try
                {
                    var zip = new ZipArchive(streamToReadFrom);
                    zip.ExtractToDirectory(UpdateStagingDirectory.FullPath, true);
                    Loggers.UpdateLogger?.Debug("{UpdatePath} unzipped successfully to {StagingPath}", UpdateFile, UpdateStagingDirectory);

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
            _cancellationTokenSource?.Cancel();
        }
        public async Task<bool> Download(JobStep? step)
        {

            return await _updateIdentity.RunAsync(async () =>
            {
                if (Release == null)
                {
                    return false;
                }

                int retries = 5;
                var progress = new FileProgress();

                while (retries > 0)
                {
                    try
                    {
                        LogDownloadAttempt();

                        using (var client = new HttpClient())
                        using (var response = await client.GetAsync(Release.DownloadURL, HttpCompletionOption.ResponseHeadersRead))
                        {
                            if (!response.IsSuccessStatusCode)
                            {
                                Loggers.UpdateLogger?.Debug("Unable to connect to download url: {@StatusCode}:{@ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                                return false;
                            }

                            UpdateDownloadDirectory.EnsureCreated();
                            if (UpdateFile.Exists)
                            {
                                UpdateFile.Delete();
                            }

                            using var streamToReadFrom = await response.Content.ReadAsStreamAsync();
                            using var streamToWriteTo = UpdateFile.OpenWriteStream();
                            progress.ExpectedSize = (int)Release.ExpectedSize.GetValueOrDefault();
                            bool result = await WriteStreamWithProgress(streamToReadFrom, streamToWriteTo, progress, step);
                            if (!result)
                            {
                                return false;
                            }
                        }
                        retries = 0;
                    }
                    catch (Exception ex)
                    {
                        retries--;
                        if (retries == 0)
                        {
                            throw new ApplicationUpdateException("Failed to download update", ex);
                        }
                        else
                        {
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
            Loggers.UpdateLogger?.Debug("Download Path: {@UpdateDirectory}", UpdateDownloadDirectory);
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
