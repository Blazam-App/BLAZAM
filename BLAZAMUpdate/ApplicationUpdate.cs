using System.Diagnostics;
using System.IO.Compression;
using System.Security.Principal;
using System.Text;
using BLAZAM.Common.Data;
using BLAZAM.Database.Context;
using BLAZAM.FileSystem;
using BLAZAM.Jobs;
using BLAZAM.Logger;
using BLAZAM.Update.Exceptions;
using BLAZAM.Update.Services;

namespace BLAZAM.Update
{
    public enum UpdateStage { None, Downloading, Downloaded, Staging, Staged, BackingUp, Prepared, Applying, Applied };

    public class ApplicationUpdate : IEquatable<ApplicationUpdate?>
    {



        public UpdateStage UpdateStage { get; set; }
        /// <summary>
        /// Token source for cancelling this update when in progress
        /// </summary>
        private CancellationTokenSource cancellationTokenSource { get; set; } = new CancellationTokenSource();

        public static AppDelegate OnUpdateStarted { get; set; }

        public static AppDelegate<Exception> OnUpdateFailed { get; set; }

        /// <summary>
        /// The version of this update
        /// </summary>
        public ApplicationVersion Version { get => Release.Version; set => Release.Version = value; }

        public string Branch { get => Release.Branch; }

        private readonly IAppDatabaseFactory _dbFactory;
        private readonly UpdateService _updateService;

        /// <summary>
        /// The application update directory, in temporary files
        /// </summary>
        /// <returns>
        /// eg: C:\user\appdata\local\temp\BLAZAM\update\
        ///</returns>
        private static SystemDirectory UpdateTempDirectory { get; set; }

        public static SystemDirectory StagingDirectory =>
            new(UpdateTempDirectory + $"staged{Path.DirectorySeparatorChar}");

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
        public static SystemDirectory UpdateDownloadDirectory
        {
            get => new(UpdateTempDirectory + $"download{Path.DirectorySeparatorChar}");
        }
        public SystemDirectory BackupPath
        {
            get => new(UpdateTempDirectory + $"backup{Path.DirectorySeparatorChar}" + _runningVersion + Path.DirectorySeparatorChar);
        }
        public SystemDirectory BackupDirectory
        {
            get => new(UpdateTempDirectory + $"backup{Path.DirectorySeparatorChar}" + _runningVersion + Path.DirectorySeparatorChar);
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
                return "cmd";
            }
        }
        public string UpdateCommandArguments
        {
            get
            {
                return "/c start Powershell -ExecutionPolicy Bypass -command \"& '" + CommandProcessPath
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
                    args += $"bin{Path.DirectorySeparatorChar}Debug{Path.DirectorySeparatorChar}net8.0{Path.DirectorySeparatorChar}";
                args += "'";

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


        public ApplicationUpdate(ApplicationInfo applicationInfo, UpdateService updateService, IAppDatabaseFactory dbFactory)
        {
            _dbFactory = dbFactory;
            _updateService = updateService;
            UpdateTempDirectory = new SystemDirectory(applicationInfo.TempDirectory + $"update{Path.DirectorySeparatorChar}");
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

        public List<Func<bool>> PreRequisiteChecks { get; private set; } = new();

        public bool PassesPrerequisiteChecks
        {
            get
            {
                if (PreRequisiteChecks.Count == 0) return true;
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

            if (cancellationTokenSource == null || cancellationTokenSource.IsCancellationRequested)
                cancellationTokenSource = new CancellationTokenSource();


            Job updateJob = new("Applying application update", "System", cancellationTokenSource);
            updateJob.StopOnFailedStep = true;
            var cleanDownloadStep = new JobStep("Cleaning previous downloads", CleanDownload);
            var downloadStep = new JobStep("Download latest version", Download);
            var cleanStageStep = new JobStep("Cleaning staging area", CleanStaging);
            var stageStep = new JobStep("Extract files", ExtractFiles);
            var stagingCheckStep = new JobStep("Check prepared files", (step) => { return UpdateStagingDirectory.Exists; });
            var bakupStep = new JobStep("Create backup", Backup);
            var updateUpdaterStep = new JobStep("Apply Files", InitiateFileCopy);
            var waitForRestart = new JobStep("Wait for completion...", Wait);
            updateJob.AddStep(cleanDownloadStep);
            updateJob.AddStep(downloadStep);
            updateJob.AddStep(cleanStageStep);
            updateJob.AddStep(stageStep);
            updateJob.AddStep(stagingCheckStep);
            updateJob.AddStep(bakupStep);
            updateJob.AddStep(updateUpdaterStep);
            updateJob.AddStep(waitForRestart);
            return updateJob;








            throw new ApplicationUpdateException("An unknown error caused the update to fail.");

        }
        private static async Task<bool> Wait(JobStep? step)
        {

            await Task.Delay(60000);
            return false;
        }
        private async Task<bool> InitiateFileCopy(JobStep? step)
        {
            //All prerequisites met


            Loggers.UpdateLogger?.Debug("Copying updater script");
            Loggers.UpdateLogger?.Debug("Source: {Source}", UpdateStagingDirectory + $"{Path.DirectorySeparatorChar}updater{Path.DirectorySeparatorChar}*");
            Loggers.UpdateLogger?.Debug("Dest: {Destination}", _applicationRootDirectory + $"updater{Path.DirectorySeparatorChar}");


            using var context = await _dbFactory.CreateDbContextAsync();
            var updateCredentials = _updateService.GetUpdateCredentials();
            if (updateCredentials != null)
            {
                return await updateCredentials.RunAsync(() =>
                {
                    try
                    {
                        return ApplyFiles();
                    }
                    catch (Exception ex)
                    {
                        Loggers.UpdateLogger?.Error(ex, "Error applying update");

                    }
                    return false;
                });
            }
            else
            {
                try
                {
                    return ApplyFiles();
                }
                catch (Exception ex)
                {
                    Loggers.UpdateLogger?.Error(ex, "Error applying update");

                }
            }
            return false;
        }

        private bool ApplyFiles()
        {
            Loggers.UpdateLogger?.Information("Running update as: {RunningUser}", WindowsIdentity.GetCurrent().Name);



            SystemDirectory updaterDirFromStagedUpdate = new(UpdateStagingDirectory.FullPath + $"updater{Path.DirectorySeparatorChar}");
            SystemDirectory updaterDir = new(_applicationRootDirectory.FullPath + $"updater{Path.DirectorySeparatorChar}");





            //Update the updater first
            updaterDirFromStagedUpdate.CopyTo(updaterDir);
            Loggers.UpdateLogger?.Information("Updater updated");

            //If the updater updated we can  run the updater
            var updaterRan = InvokeUpdateExecutable();

            if (updaterRan)
            {
                Loggers.UpdateLogger?.Information("Update process started");

                return true;
            }
            else
            {

                throw new ApplicationUpdateException("Updater script did not run.");
            }
        }

        private bool InvokeUpdateExecutable()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = UpdateCommandProcess,
                    Arguments = UpdateCommandArguments,
                    RedirectStandardOutput = true, // Enable output redirection
                    RedirectStandardError = true,
                    UseShellExecute = false,       // Required for redirection
                    CreateNoWindow = true,
                }
            };

            Loggers.UpdateLogger?.Information("Starting update process");
            process.Start();
            Loggers.UpdateLogger?.Information("Update process id: {@ProcessId}", process.Id);

            // Read and log the output asynchronously
            var output = new StringBuilder();
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    output.AppendLine(e.Data);
                    Loggers.UpdateLogger?.Information("Update process output: {@ProcessOutput}", e.Data);
                }
            };
            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    output.AppendLine(e.Data);
                    Loggers.UpdateLogger?.Error("Update process error: {@ProcessOutput}", e.Data); // Log as error
                }
            };
            process.BeginOutputReadLine(); // Start asynchronous reading

            process.WaitForExit();
            stopwatch.Stop();
            Loggers.UpdateLogger?.Information("Update process exited in {@ExeecutionTime}: {@ExitCode}", stopwatch.ElapsedMilliseconds + "ms", process.ExitCode);


            // Log the complete output (if needed)
            Loggers.UpdateLogger?.Information("Complete update process output:\n{@ProcessOutput}", output.ToString());

            return true;
        }
        public async Task<bool> Backup(JobStep? step)
        {
            Loggers.UpdateLogger?.Information("Attempting backup of current version to: {@BackupPath}", BackupPath);
            try
            {
                var result = await Task.Run(() => { return _applicationRootDirectory.CopyTo(BackupDirectory); });

                Loggers.UpdateLogger?.Debug("Backup result: {@BackupResult}", result.ToString());

                return result;
            }
            catch (Exception ex)
            {
                Loggers.UpdateLogger?.Error(ex, "Backup of current version failed");
                return false;
            }
        }


        public async Task<bool> CleanDownload(IJobStep? step)
        {
            return await Task.Run(() =>
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
            return await Task.Run(() =>
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
            return await Task.Run(() =>
            {

                if (!UpdateFile.Exists) return false;

                Loggers.UpdateLogger?.Debug("Attempting unzip of {UpdatePath}", UpdateFile);

                UpdateStagingDirectory.EnsureCreated();

                using (var streamToReadFrom = UpdateFile.OpenReadStream())
                {
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
                }
            });

        }

        public void Cancel()
        {
            cancellationTokenSource?.Cancel();
        }
        public async Task<bool> Download(JobStep? step)
        {

            if (Release == null)
            {
                return false;
            }
            int retries = 5;
            while (retries > 0)
            {
                try
                {
                    Loggers.UpdateLogger?.Debug("Attempting download of update {@UpdateVersion}", Version);
                    Loggers.UpdateLogger?.Debug("Download URL: {@DownloadURL}", Release.DownloadURL);
                    Loggers.UpdateLogger?.Debug("Download Path: {@UpdateDirectory}", UpdateDownloadDirectory);

                    var progress = new FileProgress();
                    using (var client = new HttpClient())
                    {
                        using (var response = await client.GetAsync(Release.DownloadURL, HttpCompletionOption.ResponseHeadersRead))
                        {
                            if (!response.IsSuccessStatusCode)
                            {
                                Loggers.UpdateLogger?.Debug("Unable to connect to download url: {@StatusCode}:{@ReasonPhrase}", response.StatusCode, response.ReasonPhrase);

                                return false;
                            }
                            UpdateDownloadDirectory.EnsureCreated();
                            if (UpdateFile.Exists) UpdateFile.Delete();
                            using (var streamToReadFrom = await response.Content.ReadAsStreamAsync())
                            {
                                using (var streamToWriteTo = UpdateFile.OpenWriteStream())
                                {
                                    progress.ExpectedSize = (int)Release.ExpectedSize.GetValueOrDefault();
                                    var buffer = new byte[4096];
                                    int bytesRead;
                                    int totalBytesRead = 0;

                                    while ((bytesRead = await streamToReadFrom.ReadAsync(buffer, 0, buffer.Length, cancellationTokenSource.Token)) > 0)
                                    {
                                        if (cancellationTokenSource.IsCancellationRequested != true)
                                        {
                                            await streamToWriteTo.WriteAsync(buffer, 0, bytesRead, cancellationTokenSource.Token);
                                            totalBytesRead += bytesRead;
                                            progress.CompletedBytes = totalBytesRead;
                                            if (step != null)
                                            {
                                                step.Progress = progress.FilePercentage;
                                            }

                                            DownloadPercentageChanged?.Invoke(progress);
                                        }
                                        else
                                        {
                                            DownloadPercentageChanged?.Invoke(null);

                                            return false;
                                        }
                                    }

                                    retries = 0;

                                }
                            }
                        }
                    }
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
